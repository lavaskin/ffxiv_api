using ffxiv_api.Data;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
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
	/// All logs, newest run first
	/// </summary>
	public async Task<List<MentorRouletteLogResponse>> GetAllAsync()
	{
		var logs = await db.MentorRouletteLogs
			.AsNoTracking()
			.Include(log => log.Duty)
			.OrderByDescending(log => log.SortOrder)
			.ToListAsync();

		return logs.Select(MentorRouletteLogResponse.FromEntity).ToList();
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
