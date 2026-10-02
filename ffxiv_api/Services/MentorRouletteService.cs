using System.Linq.Expressions;
using ffxiv_api.Data;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

public class MentorRouletteService(
	AppDbContext db,
	TimeProvider timeProvider,
	MentorRouletteStatsCache statsCache
)
{
	private static readonly ServiceError LogNotFound = ServiceError.NotFound("Mentor Roulette Log not found.");

	/// <summary>
	/// Enum columns sort by their value, i.e. game order (jobs grouped by role), not by label.
	/// </summary>
	private static readonly GridDefinition<MentorRouletteLog> Grid = new GridDefinition<MentorRouletteLog>()
		.SearchBy(MatchesSearch)
		.SortableBy("sortOrder", log => log.SortOrder)
		.SortableBy("playedJob", log => log.PlayedJob)
		.SortableBy("dutyName", log => log.Duty.Name)
		.SortableBy("dutyType", log => log.Duty.DutyType)
		.SortableBy("completed", log => log.Completed)
		.SortableBy("replacement", log => log.Replacement)
		.SortableBy("notes", log => log.Notes)
		.SortableBy("datePlayed", log => log.DatePlayed)
		.DefaultSort("sortOrder", SortDirection.Desc)
		.TiebreakBy(log => log.MentorRouletteLogId);

	/// <summary>
	/// One page of the roulettes grid. Newest run first unless the request sorts by something else.
	/// </summary>
	public Task<ServiceResult<PagedResponse<MentorRouletteLogResponse>>> GetPageAsync(MentorRouletteLogGridRequest request)
	{
		var logs = db.MentorRouletteLogs
			.AsNoTracking()
			.Include(log => log.Duty);

		return Grid.ToPageAsync(logs, request, MentorRouletteLogResponse.FromEntity);
	}

	/// <summary>
	/// Case-insensitive "contains" on the played job, duty name, duty type and notes.
	/// </summary>
	private static Expression<Func<MentorRouletteLog, bool>> MatchesSearch(string term)
	{
		// Lowered explicitly so matching doesn't depend on the column collation
		string lowerTerm = term.ToLowerInvariant();
		var playedJobs = EnumLabelSearch.ValuesMatching<JobEnum>(term, j => j.GetLabel());
		var dutyTypes = EnumLabelSearch.ValuesMatching<DutyTypeEnum>(term, t => t.GetLabel());

		return log => playedJobs.Contains(log.PlayedJob)
			|| log.Duty.Name.ToLower().Contains(lowerTerm)
			|| (log.Duty.DutyType != null && dutyTypes.Contains(log.Duty.DutyType.Value))
			|| log.Notes.ToLower().Contains(lowerTerm);
	}

	public async Task<ServiceResult<MentorRouletteLogResponse>> GetAsync(long logId)
	{
		var log = await db.MentorRouletteLogs
			.AsNoTracking()
			.Include(log => log.Duty)
			.FirstOrDefaultAsync(log => log.MentorRouletteLogId == logId);

		return log is null ? LogNotFound : MentorRouletteLogResponse.FromEntity(log);
	}

	public async Task<ServiceResult<MentorRouletteLogResponse>> CreateAsync(MentorRouletteLogRequest request)
	{
		var duty = await ValidateAsync(request);
		if (!duty.IsSuccess)
		{
			return duty.Error;
		}

		int maxSortOrder = await db.MentorRouletteLogs.MaxAsync(log => (int?)log.SortOrder) ?? 0;
		var log = new MentorRouletteLog
		{
			SortOrder = maxSortOrder + 1,
			DatePlayed = timeProvider.GetUtcNow().UtcDateTime,
		};
		Apply(request, duty.Value, log);

		db.MentorRouletteLogs.Add(log);
		await db.SaveChangesAsync();
		statsCache.Invalidate();

		return MentorRouletteLogResponse.FromEntity(log);
	}

	public async Task<ServiceResult<MentorRouletteLogResponse>> UpdateAsync(long logId, MentorRouletteLogRequest request)
	{
		var log = await db.MentorRouletteLogs.FindAsync(logId);
		if (log is null)
		{
			return LogNotFound;
		}

		var duty = await ValidateAsync(request);
		if (!duty.IsSuccess)
		{
			return duty.Error;
		}

		Apply(request, duty.Value, log);
		await db.SaveChangesAsync();
		statsCache.Invalidate();

		return MentorRouletteLogResponse.FromEntity(log);
	}

	/// <returns>null on success, otherwise why the log couldn't be deleted</returns>
	public async Task<ServiceError?> DeleteAsync(long logId)
	{
		var log = await db.MentorRouletteLogs.FindAsync(logId);
		if (log is null)
		{
			return LogNotFound;
		}

		db.MentorRouletteLogs.Remove(log);
		await db.SaveChangesAsync();
		statsCache.Invalidate();

		return null;
	}

	/// <summary>
	/// Served from <see cref="MentorRouletteStatsCache"/>. Recalculated only after a write clears it.
	/// </summary>
	public Task<MentorRouletteStats> GetStatsAsync()
	{
		return statsCache.GetOrCreateAsync(CalculateStatsAsync);
	}

	private async Task<MentorRouletteStats> CalculateStatsAsync()
	{
		var runs = await db.MentorRouletteLogs
			.AsNoTracking()
			.Select(log => new RouletteRun(
				log.Duty.Name,
				log.Duty.DutyType,
				log.Duty.Expansion,
				log.PlayedJob,
				log.Completed))
			.ToListAsync();

		return MentorRouletteStatsCalculator.Calculate(runs);
	}

	/// <summary>
	/// On success, returns the (tracked) duty the log points at, so it can be attached without a second query.
	/// </summary>
	private async Task<ServiceResult<Duty>> ValidateAsync(MentorRouletteLogRequest request)
	{
		string? validationError = request.Validate();
		if (validationError is not null)
		{
			return ServiceError.Invalid(validationError);
		}

		var duty = await db.Duties.FindAsync(request.DutyId);
		return duty is null
			? ServiceError.Invalid("The selected duty does not exist.")
			: duty;
	}

	/// <summary>
	/// Copies the writable fields only. SortOrder and DatePlayed are server-owned and never taken from the client.
	/// </summary>
	private static void Apply(MentorRouletteLogRequest request, Duty duty, MentorRouletteLog log)
	{
		log.Duty = duty;
		log.DutyId = duty.DutyId;
		log.PlayedJob = request.PlayedJob!.Value; // non-null once Validate() has passed
		log.Completed = request.Completed;
		log.Replacement = request.Replacement;
		log.Notes = request.Notes ?? string.Empty;
	}
}
