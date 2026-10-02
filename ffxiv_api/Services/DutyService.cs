using System.Linq.Expressions;
using ffxiv_api.Data;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

public class DutyService(AppDbContext db, MentorRouletteStatsCache statsCache)
{
	public const int MaxSearchPageSize = 25;

	private static readonly ServiceError DutyNotFound = ServiceError.NotFound("Duty not found.");

	/// <summary>
	/// Enum columns sort by their value, i.e. game order (expansions oldest first), not by label.
	/// </summary>
	private static readonly GridDefinition<Duty> Grid = new GridDefinition<Duty>()
		.SearchBy(MatchesSearch)
		.SortableBy("dutyId", d => d.DutyId)
		.SortableBy("name", d => d.Name)
		.SortableBy("levelRequirement", d => d.LevelRequirement)
		.SortableBy("expansion", d => d.Expansion)
		.SortableBy("dutyType", d => d.DutyType)
		.DefaultSort("name", SortDirection.Asc)
		.TiebreakBy(d => d.DutyId);

	/// <summary>
	/// One page of the duties grid. Alphabetical unless the request sorts by something else.
	/// </summary>
	public Task<ServiceResult<PagedResponse<DutyResponse>>> GetPageAsync(DutyGridRequest request)
	{
		return Grid.ToPageAsync(db.Duties.AsNoTracking(), request, DutyResponse.FromEntity);
	}

	/// <summary>
	/// Case-insensitive "contains" on the name, expansion, duty type and level requirement.
	/// </summary>
	private static Expression<Func<Duty, bool>> MatchesSearch(string term)
	{
		// Lowered explicitly so matching doesn't depend on the column collation
		string lowerTerm = term.ToLowerInvariant();
		var expansions = EnumLabelSearch.ValuesMatching<ExpansionEnum>(term, e => e.GetLabel());
		var dutyTypes = EnumLabelSearch.ValuesMatching<DutyTypeEnum>(term, t => t.GetLabel());

		return d => d.Name.ToLower().Contains(lowerTerm)
			|| (d.Expansion != null && expansions.Contains(d.Expansion.Value))
			|| (d.DutyType != null && dutyTypes.Contains(d.DutyType.Value))
			|| d.LevelRequirement.ToString().Contains(term);
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
