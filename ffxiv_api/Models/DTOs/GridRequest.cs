namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Paging, free-text search and sorting for a server-side grid, bound from the query string, e.g.
/// <c>?page=2&amp;pageSize=50&amp;search=aurum&amp;sortBy=name&amp;sortDirection=desc</c>.
/// </summary>
/// <remarks>
/// Each grid has its own sealed subclass, so typed filters (e.g. <c>?expansion=6</c>) can be added
/// to just that grid. The fields the search matches and the valid <see cref="SortBy"/> keys are
/// defined by the grid's <c>GridDefinition</c> in its service.
/// </remarks>
public abstract record GridRequest
{
	public const int DefaultPageSize = 50;

	public const int MaxPageSize = 200;

	/// <summary>
	/// 1-based
	/// </summary>
	public int Page { get; init; } = 1;

	public int PageSize { get; init; } = DefaultPageSize;

	/// <summary>
	/// Free text matched against the grid's searchable fields. Blank means no filter.
	/// </summary>
	public string? Search { get; init; }

	/// <summary>
	/// One of the grid's sort keys. Null uses the grid's default sort, and ignores <see cref="SortDirection"/>.
	/// </summary>
	public string? SortBy { get; init; }

	public SortDirection SortDirection { get; init; } = SortDirection.Asc;

	public string? NormalizedSearch => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

	public string? NormalizedSortBy => string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim();

	/// <summary>
	/// Rules that don't depend on the grid. Returns the first problem found, or null if valid.
	/// </summary>
	public string? Validate()
	{
		if (Page < 1)
		{
			return "Page must be 1 or greater.";
		}

		if (PageSize < 1 || PageSize > MaxPageSize)
		{
			return $"Page size must be between 1 and {MaxPageSize}.";
		}

		if ((long)(Page - 1) * PageSize > int.MaxValue)
		{
			return "Page is out of range.";
		}

		if (!Enum.IsDefined(SortDirection))
		{
			return "Sort direction must be 'asc' or 'desc'.";
		}

		return null;
	}
}
