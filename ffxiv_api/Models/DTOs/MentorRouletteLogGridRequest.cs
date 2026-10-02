namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Query string for the roulettes grid. Add log-only filters here (e.g. <c>bool? Completed</c>)
/// and apply them in <c>MentorRouletteService.GetPageAsync</c>.
/// </summary>
public sealed record MentorRouletteLogGridRequest : GridRequest;
