using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using ffxiv_api.Services;

namespace ffxiv_api.Tests;

/// <summary>
/// Paging, sorting and validation shared by every grid. Each grid's own search fields and sort
/// keys are tested alongside its service.
/// </summary>
public sealed class GridDefinitionTests : IDisposable
{
	private static readonly GridDefinition<Duty> Grid = new GridDefinition<Duty>()
		.SearchBy(term => d => d.Name.Contains(term))
		.SortableBy("name", d => d.Name)
		.SortableBy("level", d => d.LevelRequirement)
		.DefaultSort("name", SortDirection.Asc)
		.TiebreakBy(d => d.DutyId);

	private readonly TestDatabase _db = new();

	public void Dispose() => _db.Dispose();

	private static Duty Duty(string name, long level = 50) => new()
	{
		Name = name,
		DutyType = DutyTypeEnum.Dungeon,
		Expansion = ExpansionEnum.ARealmReborn,
		LevelRequirement = level,
	};

	private async Task<ServiceResult<PagedResponse<T>>> Query<T>(DutyGridRequest request, Func<Duty, T> toResult)
	{
		await using var context = _db.CreateContext();
		return await Grid.ToPageAsync(context.Duties, request, toResult);
	}

	private async Task<PagedResponse<string>> Names(DutyGridRequest request)
	{
		var result = await Query(request, d => d.Name);
		Assert.True(result.IsSuccess, result.Error?.Message);
		return result.Value;
	}

	[Fact]
	public async Task Pages_are_cut_after_sorting_and_report_the_total()
	{
		_db.Seed(Duty("Delta"), Duty("Charlie"), Duty("Echo"), Duty("Alpha"), Duty("Bravo"));

		var page = await Names(new DutyGridRequest { Page = 2, PageSize = 2 });

		Assert.Equal(["Charlie", "Delta"], page.Items);
		Assert.Equal(2, page.Page);
		Assert.Equal(2, page.PageSize);
		Assert.Equal(5, page.TotalCount);
	}

	[Fact]
	public async Task A_page_past_the_end_is_empty_but_keeps_the_total()
	{
		_db.Seed(Duty("Alpha"), Duty("Bravo"));

		var page = await Names(new DutyGridRequest { Page = 3, PageSize = 2 });

		Assert.Empty(page.Items);
		Assert.Equal(3, page.Page);
		Assert.Equal(2, page.TotalCount);
	}

	[Fact]
	public async Task Defaults_to_the_first_page_of_fifty()
	{
		_db.Seed(Enumerable.Range(0, 60).Select(i => (object)Duty($"Duty {i:00}")).ToArray());

		var page = await Names(new DutyGridRequest());

		Assert.Equal(GridRequest.DefaultPageSize, page.Items.Count);
		Assert.Equal("Duty 00", page.Items[0]);
		Assert.Equal(60, page.TotalCount);
	}

	[Fact]
	public async Task Search_filters_before_counting_and_paging()
	{
		_db.Seed(Duty("Sastasha"), Duty("Halatali"), Duty("Brayflox's Longstop"), Duty("Tam-Tara Deepcroft"));

		var page = await Names(new DutyGridRequest { Search = "ta", PageSize = 1 });

		Assert.Equal(["Halatali"], page.Items);
		Assert.Equal(2, page.TotalCount);
	}

	[Fact]
	public async Task Search_is_trimmed()
	{
		_db.Seed(Duty("Sastasha"), Duty("Halatali"));

		var page = await Names(new DutyGridRequest { Search = "  Sasta  " });

		Assert.Equal(["Sastasha"], page.Items);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task Blank_search_does_not_filter(string? search)
	{
		_db.Seed(Duty("Sastasha"), Duty("Halatali"));

		var page = await Names(new DutyGridRequest { Search = search });

		Assert.Equal(2, page.TotalCount);
	}

	[Fact]
	public async Task Sorts_by_the_requested_key_and_direction()
	{
		_db.Seed(Duty("Alpha", level: 15), Duty("Bravo", level: 50), Duty("Charlie", level: 32));

		var page = await Names(new DutyGridRequest { SortBy = "level", SortDirection = SortDirection.Desc });

		Assert.Equal(["Bravo", "Charlie", "Alpha"], page.Items);
	}

	[Fact]
	public async Task Sort_keys_ignore_case_and_whitespace()
	{
		_db.Seed(Duty("Alpha", level: 50), Duty("Bravo", level: 15));

		var page = await Names(new DutyGridRequest { SortBy = " LEVEL " });

		Assert.Equal(["Bravo", "Alpha"], page.Items);
	}

	[Fact]
	public async Task Ties_fall_back_to_the_default_sort()
	{
		_db.Seed(Duty("Charlie", level: 50), Duty("Alpha", level: 50), Duty("Bravo", level: 60));

		var page = await Names(new DutyGridRequest { SortBy = "level", SortDirection = SortDirection.Desc });

		// Level descending, then name in the default (ascending) direction
		Assert.Equal(["Bravo", "Alpha", "Charlie"], page.Items);
	}

	[Fact]
	public async Task Exact_ties_are_ordered_by_the_tiebreaker_in_the_sort_direction()
	{
		var first = Duty("Sastasha");
		var second = Duty("Sastasha");
		_db.Seed(first, second);

		var ascending = await Query(new DutyGridRequest { SortBy = "name" }, d => d.DutyId);
		var descending = await Query(new DutyGridRequest { SortBy = "name", SortDirection = SortDirection.Desc }, d => d.DutyId);

		Assert.Equal([first.DutyId, second.DutyId], ascending.Value!.Items);
		Assert.Equal([second.DutyId, first.DutyId], descending.Value!.Items);
	}

	[Fact]
	public async Task Without_a_sort_key_the_default_sort_is_used_and_the_direction_ignored()
	{
		_db.Seed(Duty("Bravo"), Duty("Alpha"));

		var page = await Names(new DutyGridRequest { SortDirection = SortDirection.Desc });

		Assert.Equal(["Alpha", "Bravo"], page.Items);
	}

	[Fact]
	public async Task Rejects_an_unknown_sort_key_and_lists_the_valid_ones()
	{
		var result = await Query(new DutyGridRequest { SortBy = "dutyModel.name" }, d => d.Name);

		Assert.Equal(ServiceErrorKind.Invalid, result.Error?.Kind);
		Assert.Contains("'dutyModel.name'", result.Error!.Message);
		Assert.Contains("name, level", result.Error.Message);
	}

	[Theory]
	[InlineData(0, 50)]
	[InlineData(-1, 50)]
	[InlineData(1, 0)]
	[InlineData(1, GridRequest.MaxPageSize + 1)]
	[InlineData(int.MaxValue, GridRequest.MaxPageSize)]
	public async Task Rejects_out_of_range_paging(int page, int pageSize)
	{
		var result = await Query(new DutyGridRequest { Page = page, PageSize = pageSize }, d => d.Name);

		Assert.Equal(ServiceErrorKind.Invalid, result.Error?.Kind);
	}

	[Fact]
	public async Task Rejects_an_undefined_sort_direction()
	{
		var result = await Query(new DutyGridRequest { SortBy = "name", SortDirection = (SortDirection)5 }, d => d.Name);

		Assert.Equal(ServiceErrorKind.Invalid, result.Error?.Kind);
	}

	[Fact]
	public async Task Accepts_the_max_page_size()
	{
		var result = await Query(new DutyGridRequest { PageSize = GridRequest.MaxPageSize }, d => d.Name);

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void The_default_sort_must_be_a_registered_key()
	{
		Assert.Throws<InvalidOperationException>(() => new GridDefinition<Duty>().DefaultSort("name", SortDirection.Asc));
	}
}
