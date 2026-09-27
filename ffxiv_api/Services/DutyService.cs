using ffxiv_api.Data;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

public class DutyService(AppDbContext db, MentorRouletteStatsCache statsCache)
{
	public const int MaxSearchPageSize = 25;

	private static readonly ServiceError DutyNotFound = ServiceError.NotFound("Duty not found.");

	public async Task<List<DutyResponse>> GetAllAsync()
	{
		var duties = await db.Duties
			.AsNoTracking()
			.OrderBy(d => d.Name)
			.ToListAsync();

		return duties.Select(DutyResponse.FromEntity).ToList();
	}

	public async Task<ServiceResult<DutyResponse>> GetAsync(long dutyId)
	{
		var duty = await db.Duties
			.AsNoTracking()
			.FirstOrDefaultAsync(d => d.DutyId == dutyId);

		return duty is null ? DutyNotFound : DutyResponse.FromEntity(duty);
	}

	/// <summary>
	/// Alphabetical, paged name search for the duty picker.
	/// </summary>
	public async Task<List<ListResultItem>> SearchAsync(SearchOptions options)
	{
		int pageSize = Math.Clamp(options.PageSize, 1, MaxSearchPageSize);
		int page = Math.Max(options.Page, 1);

		var query = db.Duties.AsNoTracking();
		if (!string.IsNullOrWhiteSpace(options.Query))
		{
			string term = options.Query.Trim();
			query = query.Where(d => d.Name.Contains(term));
		}

		return await query
			.OrderBy(d => d.Name)
			.ThenBy(d => d.DutyId)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(d => new ListResultItem { Label = d.Name, Value = d.DutyId })
			.ToListAsync();
	}

	public async Task<ServiceResult<DutyResponse>> CreateAsync(DutyRequest request)
	{
		var error = await ValidateAsync(request, dutyIdBeingUpdated: null);
		if (error is not null)
		{
			return error;
		}

		var duty = new Duty();
		Apply(request, duty);

		db.Duties.Add(duty);
		await db.SaveChangesAsync();

		return DutyResponse.FromEntity(duty);
	}

	public async Task<ServiceResult<DutyResponse>> UpdateAsync(long dutyId, DutyRequest request)
	{
		var duty = await db.Duties.FindAsync(dutyId);
		if (duty is null)
		{
			return DutyNotFound;
		}

		var error = await ValidateAsync(request, dutyIdBeingUpdated: dutyId);
		if (error is not null)
		{
			return error;
		}

		Apply(request, duty);
		await db.SaveChangesAsync();

		// Duty name, type and expansion feed the stats. Creating or deleting a duty doesn't need
		// this: new duties have no logs, and duties with logs can't be deleted.
		statsCache.Invalidate();

		return DutyResponse.FromEntity(duty);
	}

	/// <returns>null on success, otherwise why the duty couldn't be deleted</returns>
	public async Task<ServiceError?> DeleteAsync(long dutyId)
	{
		var duty = await db.Duties.FindAsync(dutyId);
		if (duty is null)
		{
			return DutyNotFound;
		}

		bool hasLogs = await db.MentorRouletteLogs.AnyAsync(log => log.DutyId == dutyId);
		if (hasLogs)
		{
			return ServiceError.Conflict("Cannot delete duty with existing logs.");
		}

		db.Duties.Remove(duty);
		await db.SaveChangesAsync();

		return null;
	}

	private async Task<ServiceError?> ValidateAsync(DutyRequest request, long? dutyIdBeingUpdated)
	{
		string? validationError = request.Validate();
		if (validationError is not null)
		{
			return ServiceError.Invalid(validationError);
		}

		// The column's collation is case-insensitive, but normalize explicitly so the intent
		// doesn't depend on database configuration.
		string normalizedName = request.NormalizedName.ToLower();
		var sameName = db.Duties.Where(d => d.Name.ToLower() == normalizedName);
		if (dutyIdBeingUpdated is { } dutyId)
		{
			sameName = sameName.Where(d => d.DutyId != dutyId);
		}

		return await sameName.AnyAsync()
			? ServiceError.Conflict("A duty with this name already exists.")
			: null;
	}

	private static void Apply(DutyRequest request, Duty duty)
	{
		duty.Name = request.NormalizedName;
		duty.DutyType = request.DutyType;
		duty.Expansion = request.Expansion;
		duty.LevelRequirement = request.LevelRequirement;
	}
}
