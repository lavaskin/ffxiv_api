using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Query string for the roulettes grid, e.g.
/// <c>?expansions=5&amp;expansions=6&amp;subRoles=1&amp;completed=true&amp;playedFrom=2026-03-01T05:00:00Z</c>.
/// Filters are applied in <c>MentorRouletteService.GetPageAsync</c>.
/// </summary>
/// <remarks>
/// Every filter is optional and they're all combined with AND, including with <see cref="GridRequest.Search"/>.
/// A list filter matches any of its values, and a missing or empty list means no filter.
/// </remarks>
public sealed record MentorRouletteLogGridRequest : GridRequest
{
	// The list filters must stay null by default. Given an existing (read-only) empty list, MVC's
	// collection binder tries to add the bound values to it and silently drops them when it can't.

	/// <summary>
	/// The duty's expansion. Duties without one never match.
	/// </summary>
	public IReadOnlyList<ExpansionEnum>? Expansions { get; init; }

	/// <summary>
	/// The duty's type. Duties without one never match.
	/// </summary>
	public IReadOnlyList<DutyTypeEnum>? DutyTypes { get; init; }

	/// <summary>
	/// The played job's sub-role. Combined with <see cref="Jobs"/> using AND, like every other filter.
	/// </summary>
	public IReadOnlyList<JobSubRoleEnum>? SubRoles { get; init; }

	public IReadOnlyList<JobEnum>? Jobs { get; init; }

	public bool? Completed { get; init; }

	public bool? Replacement { get; init; }

	/// <summary>
	/// Inclusive lower bound on <c>DatePlayed</c>. The client sends local midnight as an exact instant.
	/// </summary>
	public DateTimeOffset? PlayedFrom { get; init; }

	/// <summary>
	/// Exclusive upper bound on <c>DatePlayed</c>, so the client sends the midnight <em>after</em> its last day.
	/// </summary>
	public DateTimeOffset? PlayedBefore { get; init; }

	/// <summary>
	/// Model binding already rejects ids that aren't in the enums (a 400 before the controller runs),
	/// so only the date range needs checking here.
	/// </summary>
	public override string? Validate()
	{
		return base.Validate() ?? ValidatePlayedRange();
	}

	private string? ValidatePlayedRange()
	{
		return PlayedFrom is { } from && PlayedBefore is { } before && from >= before
			? "The played-from date must be before the played-before date."
			: null;
	}
}
