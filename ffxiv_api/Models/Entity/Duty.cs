using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.Entity;

/// <summary>
/// A row in the <c>duty</c> table. Persistence only: never bind this from a request or return it
/// from an endpoint. Use <see cref="DTOs.DutyRequest"/> / <see cref="DTOs.DutyResponse"/> instead.
/// Mapping lives in <see cref="Data.Configurations.DutyConfiguration"/>.
/// </summary>
public class Duty
{
	public long DutyId { get; set; }

	public string Name { get; set; } = string.Empty;

	public DutyTypeEnum? DutyType { get; set; }

	public ExpansionEnum? Expansion { get; set; }

	public long LevelRequirement { get; set; }
}
