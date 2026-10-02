namespace ffxiv_api.Models.DTOs;

/// <summary>
/// One page of a grid.
/// </summary>
public sealed record PagedResponse<T>
{
	public IReadOnlyList<T> Items { get; init; } = [];

	/// <summary>
	/// 1-based. Echoes the request, even when it's past the last page (Items is then empty).
	/// </summary>
	public int Page { get; init; }

	public int PageSize { get; init; }

	/// <summary>
	/// Rows matching the search and filters, across all pages
	/// </summary>
	public int TotalCount { get; init; }
}
