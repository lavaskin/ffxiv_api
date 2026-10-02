using System.Linq.Expressions;
using ffxiv_api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

/// <summary>
/// How one server-side grid is searched and sorted. Declare one per grid as a static field in its
/// service, then pass <see cref="ToPageAsync"/> the grid's base query (with includes and any typed
/// filters already applied). Search, sort and paging all run in the database.
/// </summary>
/// <remarks>
/// Rows are ordered by the requested sort, then the default sort, then the tiebreaker. So rows
/// with equal values keep a sensible order, and paging is deterministic: a row can't appear on two pages.
/// </remarks>
public sealed class GridDefinition<TEntity>
{
	private readonly Dictionary<string, SortColumn> _sortColumns = new(StringComparer.OrdinalIgnoreCase);
	private Func<string, Expression<Func<TEntity, bool>>>? _search;
	private (string Key, SortDirection Direction)? _defaultSort;
	private SortColumn? _tiebreaker;

	public IReadOnlyCollection<string> SortKeys => _sortColumns.Keys;

	/// <param name="predicate">Builds the filter for a trimmed, non-empty search term</param>
	public GridDefinition<TEntity> SearchBy(Func<string, Expression<Func<TEntity, bool>>> predicate)
	{
		_search = predicate;
		return this;
	}

	/// <param name="key">What clients send as <see cref="GridRequest.SortBy"/>. Matched ignoring case.</param>
	public GridDefinition<TEntity> SortableBy<TKey>(string key, Expression<Func<TEntity, TKey>> selector)
	{
		_sortColumns.Add(key, SortColumn.For(selector));
		return this;
	}

	/// <summary>
	/// Used when the request has no sort, and to order ties in any other sort.
	/// </summary>
	public GridDefinition<TEntity> DefaultSort(string key, SortDirection direction)
	{
		if (!_sortColumns.ContainsKey(key))
		{
			throw new InvalidOperationException($"Default sort '{key}' must be registered with {nameof(SortableBy)} first.");
		}

		_defaultSort = (key, direction);
		return this;
	}

	/// <summary>
	/// A unique column (normally the primary key), sorted last so the order is total.
	/// </summary>
	public GridDefinition<TEntity> TiebreakBy<TKey>(Expression<Func<TEntity, TKey>> selector)
	{
		_tiebreaker = SortColumn.For(selector);
		return this;
	}

	/// <param name="toResult">Runs in memory on the page's rows, so it can use code EF can't translate</param>
	public async Task<ServiceResult<PagedResponse<TResult>>> ToPageAsync<TResult>(
		IQueryable<TEntity> query,
		GridRequest request,
		Func<TEntity, TResult> toResult)
	{
		var (defaultKey, defaultDirection) = _defaultSort
			?? throw new InvalidOperationException($"Call {nameof(DefaultSort)} when defining the grid.");
		var tiebreaker = _tiebreaker
			?? throw new InvalidOperationException($"Call {nameof(TiebreakBy)} when defining the grid.");

		string? validationError = request.Validate();
		if (validationError is not null)
		{
			return ServiceError.Invalid(validationError);
		}

		var (sortKey, sortDirection) = request.NormalizedSortBy is { } requestedKey
			? (requestedKey, request.SortDirection)
			: (defaultKey, defaultDirection);

		if (!_sortColumns.TryGetValue(sortKey, out var sortColumn))
		{
			return ServiceError.Invalid($"Cannot sort by '{sortKey}'. Sortable fields are: {string.Join(", ", SortKeys)}.");
		}

		if (_search is not null && request.NormalizedSearch is { } term)
		{
			query = query.Where(_search(term));
		}

		int totalCount = await query.CountAsync();

		var ordered = sortColumn.OrderBy(query, sortDirection);
		var tiebreakDirection = sortDirection;
		if (!string.Equals(sortKey, defaultKey, StringComparison.OrdinalIgnoreCase))
		{
			ordered = _sortColumns[defaultKey].ThenBy(ordered, defaultDirection);
			tiebreakDirection = defaultDirection;
		}
		ordered = tiebreaker.ThenBy(ordered, tiebreakDirection);

		var rows = await ordered
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync();

		return new PagedResponse<TResult>
		{
			Items = rows.Select(toResult).ToList(),
			Page = request.Page,
			PageSize = request.PageSize,
			TotalCount = totalCount,
		};
	}

	/// <summary>
	/// Keeps the typed key selector, so EF translates the sort to ORDER BY on the real column.
	/// </summary>
	private sealed record SortColumn(
		Func<IQueryable<TEntity>, SortDirection, IOrderedQueryable<TEntity>> OrderBy,
		Func<IOrderedQueryable<TEntity>, SortDirection, IOrderedQueryable<TEntity>> ThenBy)
	{
		public static SortColumn For<TKey>(Expression<Func<TEntity, TKey>> selector) => new(
			(query, direction) => direction == SortDirection.Desc ? query.OrderByDescending(selector) : query.OrderBy(selector),
			(query, direction) => direction == SortDirection.Desc ? query.ThenByDescending(selector) : query.ThenBy(selector));
	}
}
