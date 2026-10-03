namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Query string for the duties grid. Add duty-only filters here (e.g. <c>IReadOnlyList&lt;ExpansionEnum&gt;? Expansions</c>)
/// and apply them in <c>DutyService.GetPageAsync</c>. See <see cref="MentorRouletteLogGridRequest"/>.
/// </summary>
public sealed record DutyGridRequest : GridRequest;
