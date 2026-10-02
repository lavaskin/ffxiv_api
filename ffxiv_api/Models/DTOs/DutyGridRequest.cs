namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Query string for the duties grid. Add duty-only filters here (e.g. <c>ExpansionEnum? Expansion</c>)
/// and apply them in <c>DutyService.GetPageAsync</c>.
/// </summary>
public sealed record DutyGridRequest : GridRequest;
