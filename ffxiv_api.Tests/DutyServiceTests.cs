using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using ffxiv_api.Services;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Tests;

public sealed class DutyServiceTests : IDisposable
{
	private readonly TestDatabase _db = new();
	private readonly MentorRouletteStatsCache _statsCache = new();

	public void Dispose() => _db.Dispose();

	private DutyService CreateService() => new(_db.CreateContext(), _statsCache);

	private MentorRouletteService CreateMentorRouletteService() => new(_db.CreateContext(), TimeProvider.System, _statsCache);

	private static Duty Dungeon(string name, long level = 15) => new()
	{
		Name = name,
		DutyType = DutyTypeEnum.Dungeon,
		Expansion = ExpansionEnum.ARealmReborn,
		LevelRequirement = level,
	};

	private static DutyRequest Request(string name, long level = 15) => new()
	{
		Name = name,
		DutyType = DutyTypeEnum.Dungeon,
		Expansion = ExpansionEnum.ARealmReborn,
		LevelRequirement = level,
	};

	private static MentorRouletteLog LogFor(Duty duty) => new()
	{
		Duty = duty,
		PlayedJob = JobEnum.Paladin,
		SortOrder = 1,
		DatePlayed = DateTime.UtcNow,
	};

	[Fact]
	public async Task GetAll_returns_duties_alphabetically_with_labels()
	{
		_db.Seed(Dungeon("Sastasha"), Dungeon("Halatali"), Dungeon("Copperbell Mines"));

		var duties = await CreateService().GetAllAsync();

		Assert.Equal(["Copperbell Mines", "Halatali", "Sastasha"], duties.Select(d => d.Name));
		Assert.All(duties, d => Assert.Equal("Dungeon", d.DutyTypeLabel));
		Assert.All(duties, d => Assert.Equal("2.0: A Realm Reborn", d.ExpansionLabel));
	}

	[Fact]
	public async Task Get_returns_not_found_for_a_missing_duty()
	{
		var result = await CreateService().GetAsync(12345);

		Assert.Equal(ServiceErrorKind.NotFound, result.Error?.Kind);
	}

	[Fact]
	public async Task Search_sorts_before_paging()
	{
		// Inserted out of order: the old implementation took the first N rows by id, then sorted them
		_db.Seed(Dungeon("Delta"), Dungeon("Charlie"), Dungeon("Echo"), Dungeon("Alpha"), Dungeon("Bravo"));
		var service = CreateService();

		var page1 = await service.SearchAsync(new SearchOptions { PageSize = 2, Page = 1 });
		var page2 = await service.SearchAsync(new SearchOptions { PageSize = 2, Page = 2 });

		Assert.Equal(["Alpha", "Bravo"], page1.Select(r => r.Label));
		Assert.Equal(["Charlie", "Delta"], page2.Select(r => r.Label));
	}

	[Fact]
	public async Task Search_filters_by_name_and_returns_ids()
	{
		_db.Seed(Dungeon("The Praetorium"), Dungeon("Sastasha"), Dungeon("The Porta Decumana"));

		var results = await CreateService().SearchAsync(new SearchOptions { Query = " The P " });

		Assert.Equal(["The Porta Decumana", "The Praetorium"], results.Select(r => r.Label));
		Assert.All(results, r => Assert.True(r.Value > 0));
	}

	[Fact]
	public async Task Search_page_size_is_capped()
	{
		_db.Seed(Enumerable.Range(0, 30).Select(i => (object)Dungeon($"Duty {i:00}")).ToArray());

		var results = await CreateService().SearchAsync(new SearchOptions { PageSize = 500 });

		Assert.Equal(DutyService.MaxSearchPageSize, results.Count);
	}

	[Fact]
	public async Task Create_saves_a_trimmed_name_and_returns_labels()
	{
		var result = await CreateService().CreateAsync(Request("  Sastasha  "));

		Assert.True(result.IsSuccess);
		Assert.Equal("Sastasha", result.Value.Name);
		Assert.Equal("Dungeon", result.Value.DutyTypeLabel);

		await using var context = _db.CreateContext();
		var saved = await context.Duties.SingleAsync();
		Assert.Equal(result.Value.DutyId, saved.DutyId);
		Assert.Equal("Sastasha", saved.Name);
	}

	[Fact]
	public async Task Create_rejects_a_duplicate_name_ignoring_case_and_whitespace()
	{
		_db.Seed(Dungeon("Sastasha"));

		var result = await CreateService().CreateAsync(Request(" SASTASHA "));

		Assert.Equal(ServiceErrorKind.Conflict, result.Error?.Kind);
		await using var context = _db.CreateContext();
		Assert.Equal(1, await context.Duties.CountAsync());
	}

	[Fact]
	public async Task Create_rejects_an_invalid_duty_without_saving()
	{
		var result = await CreateService().CreateAsync(Request("Sastasha", level: 99));

		Assert.Equal(ServiceErrorKind.Invalid, result.Error?.Kind);
		await using var context = _db.CreateContext();
		Assert.False(await context.Duties.AnyAsync());
	}

	[Fact]
	public async Task Update_changes_the_duty()
	{
		var duty = Dungeon("Sastasha");
		_db.Seed(duty);

		var result = await CreateService().UpdateAsync(duty.DutyId, Request("Sastasha (Hard)", level: 50));

		Assert.True(result.IsSuccess);
		await using var context = _db.CreateContext();
		var saved = await context.Duties.SingleAsync();
		Assert.Equal("Sastasha (Hard)", saved.Name);
		Assert.Equal(50, saved.LevelRequirement);
	}

	[Fact]
	public async Task Update_allows_keeping_the_same_name()
	{
		var duty = Dungeon("Sastasha");
		_db.Seed(duty);

		var result = await CreateService().UpdateAsync(duty.DutyId, Request("Sastasha", level: 16));

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task Update_rejects_renaming_to_another_duty_name()
	{
		var sastasha = Dungeon("Sastasha");
		_db.Seed(sastasha, Dungeon("Halatali"));

		var result = await CreateService().UpdateAsync(sastasha.DutyId, Request("halatali"));

		Assert.Equal(ServiceErrorKind.Conflict, result.Error?.Kind);
		await using var context = _db.CreateContext();
		Assert.Equal("Sastasha", (await context.Duties.FindAsync(sastasha.DutyId))!.Name);
	}

	[Fact]
	public async Task Updating_a_duty_refreshes_the_stats()
	{
		var duty = Dungeon("Sastasha");
		_db.Seed(LogFor(duty));
		var before = await CreateMentorRouletteService().GetStatsAsync();

		await CreateService().UpdateAsync(duty.DutyId, Request("Sastasha (Hard)", level: 50));

		var after = await CreateMentorRouletteService().GetStatsAsync();
		Assert.Equal("Sastasha", before.TopSeenDuties[0].DutyName);
		Assert.Equal("Sastasha (Hard)", after.TopSeenDuties[0].DutyName);
	}

	[Fact]
	public async Task Update_returns_not_found_for_a_missing_duty()
	{
		var result = await CreateService().UpdateAsync(12345, Request("Sastasha"));

		Assert.Equal(ServiceErrorKind.NotFound, result.Error?.Kind);
	}

	[Fact]
	public async Task Delete_removes_a_duty_without_logs()
	{
		var duty = Dungeon("Sastasha");
		_db.Seed(duty);

		var error = await CreateService().DeleteAsync(duty.DutyId);

		Assert.Null(error);
		await using var context = _db.CreateContext();
		Assert.False(await context.Duties.AnyAsync());
	}

	[Fact]
	public async Task Delete_refuses_a_duty_that_has_logs()
	{
		var duty = Dungeon("Sastasha");
		_db.Seed(LogFor(duty));

		var error = await CreateService().DeleteAsync(duty.DutyId);

		Assert.Equal(ServiceErrorKind.Conflict, error?.Kind);
		await using var context = _db.CreateContext();
		Assert.True(await context.Duties.AnyAsync());
	}

	[Fact]
	public async Task Delete_returns_not_found_for_a_missing_duty()
	{
		var error = await CreateService().DeleteAsync(12345);

		Assert.Equal(ServiceErrorKind.NotFound, error?.Kind);
	}
}
