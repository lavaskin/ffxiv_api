using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using ffxiv_api.Services;
using static ffxiv_api.Tests.TestData;

namespace ffxiv_api.Tests;

/// <summary>
/// The typed filters on the roulettes grid (<see cref="MentorRouletteLogGridRequest"/>). Every test runs
/// against the same five runs, so the expected run numbers can be read straight off <see cref="SeedRuns"/>.
/// </summary>
public sealed class MentorRouletteFilterTests : IDisposable
{
	private readonly TestDatabase _db = new();
	private readonly Duty _sastasha = Duty("Sastasha", DutyTypeEnum.Dungeon, ExpansionEnum.ARealmReborn);
	private readonly Duty _ifrit = Duty("The Bowl of Embers (Extreme)", DutyTypeEnum.ExtremeTrial, ExpansionEnum.ARealmReborn);
	private readonly Duty _zot = Duty("The Tower of Zot", DutyTypeEnum.Dungeon, ExpansionEnum.Endwalker);
	private readonly Duty _worqor = Duty("Worqor Lar Dor", DutyTypeEnum.Trial, ExpansionEnum.Dawntrail);

	/// <summary>
	/// The columns are nullable in the database, so older rows can have neither
	/// </summary>
	private readonly Duty _uncategorised = new() { Name = "Uncategorised", LevelRequirement = 1 };

	public MentorRouletteFilterTests()
	{
		_db.Seed(_sastasha, _ifrit, _zot, _worqor, _uncategorised);
		SeedRuns();
	}

	public void Dispose() => _db.Dispose();

	private MentorRouletteService CreateService() => new(_db.CreateContext(), TimeProvider.System, new MentorRouletteStatsCache());

	private static DateTime Utc(int month, int day, int hour = 0, int minute = 0) => new(2026, month, day, hour, minute, 0, DateTimeKind.Utc);

	private void SeedRuns()
	{
		_db.Seed(
			Run(1, _sastasha, JobEnum.Paladin, completed: true, replacement: false, Utc(2, 1)),
			Run(2, _ifrit, JobEnum.WhiteMage, completed: false, replacement: true, Utc(3, 1)),
			Run(3, _zot, JobEnum.Sage, completed: true, replacement: true, Utc(3, 15)),
			Run(4, _worqor, JobEnum.Dragoon, completed: true, replacement: false, Utc(3, 31, 23, 59)),
			Run(5, _uncategorised, JobEnum.Bard, completed: false, replacement: false, Utc(4, 1)));
	}

	private static MentorRouletteLog Run(int sortOrder, Duty duty, JobEnum job, bool completed, bool replacement, DateTime datePlayed) => new()
	{
		DutyId = duty.DutyId,
		SortOrder = sortOrder,
		PlayedJob = job,
		Completed = completed,
		Replacement = replacement,
		DatePlayed = datePlayed,
	};

	/// <returns>The matching run numbers, newest first (the default sort)</returns>
	private async Task<int[]> Matching(MentorRouletteLogGridRequest request)
	{
		var result = await CreateService().GetPageAsync(request);

		Assert.True(result.IsSuccess, result.Error?.Message);
		Assert.Equal(result.Value.Items.Count, result.Value.TotalCount); // everything fits on one page
		return result.Value.Items.Select(l => l.SortOrder).ToArray();
	}

	private async Task AssertMatches(int[] expected, MentorRouletteLogGridRequest request)
	{
		Assert.Equal(expected, await Matching(request));
	}

	[Fact]
	public async Task No_filters_returns_every_run()
	{
		await AssertMatches([5, 4, 3, 2, 1], new MentorRouletteLogGridRequest());
	}

	[Theory]
	[InlineData(new[] { ExpansionEnum.Endwalker }, new[] { 3 })]
	[InlineData(new[] { ExpansionEnum.Endwalker, ExpansionEnum.Dawntrail }, new[] { 4, 3 })]
	[InlineData(new[] { ExpansionEnum.ARealmReborn }, new[] { 2, 1 })]
	[InlineData(new[] { ExpansionEnum.Heavensward }, new int[0])]
	public async Task Expansion_filter_matches_any_selected_expansion(ExpansionEnum[] expansions, int[] expected)
	{
		await AssertMatches(expected, new MentorRouletteLogGridRequest { Expansions = expansions });
	}

	[Theory]
	[InlineData(new[] { DutyTypeEnum.Dungeon }, new[] { 3, 1 })]
	[InlineData(new[] { DutyTypeEnum.Trial, DutyTypeEnum.ExtremeTrial }, new[] { 4, 2 })]
	[InlineData(new[] { DutyTypeEnum.UltimateRaid }, new int[0])]
	public async Task Duty_type_filter_matches_any_selected_type(DutyTypeEnum[] dutyTypes, int[] expected)
	{
		await AssertMatches(expected, new MentorRouletteLogGridRequest { DutyTypes = dutyTypes });
	}

	[Fact]
	public async Task Duties_without_an_expansion_or_type_never_match_those_filters()
	{
		var everyExpansion = Enum.GetValues<ExpansionEnum>();
		var everyDutyType = Enum.GetValues<DutyTypeEnum>();

		Assert.DoesNotContain(5, await Matching(new MentorRouletteLogGridRequest { Expansions = everyExpansion }));
		Assert.DoesNotContain(5, await Matching(new MentorRouletteLogGridRequest { DutyTypes = everyDutyType }));
	}

	[Theory]
	[InlineData(new[] { JobSubRoleEnum.Tank }, new[] { 1 })]
	[InlineData(new[] { JobSubRoleEnum.Healer }, new[] { 3, 2 })]
	[InlineData(new[] { JobSubRoleEnum.MeleeDps, JobSubRoleEnum.PhysicalRangedDps }, new[] { 5, 4 })]
	[InlineData(new[] { JobSubRoleEnum.MagicalRangedDps }, new int[0])]
	public async Task Sub_role_filter_matches_jobs_in_any_selected_sub_role(JobSubRoleEnum[] subRoles, int[] expected)
	{
		await AssertMatches(expected, new MentorRouletteLogGridRequest { SubRoles = subRoles });
	}

	[Theory]
	[InlineData(new[] { JobEnum.Sage }, new[] { 3 })]
	[InlineData(new[] { JobEnum.Paladin, JobEnum.Bard }, new[] { 5, 1 })]
	[InlineData(new[] { JobEnum.Viper }, new int[0])]
	public async Task Job_filter_matches_any_selected_job(JobEnum[] jobs, int[] expected)
	{
		await AssertMatches(expected, new MentorRouletteLogGridRequest { Jobs = jobs });
	}

	[Fact]
	public async Task Sub_role_and_job_filters_must_both_match()
	{
		await AssertMatches([2], new MentorRouletteLogGridRequest
		{
			SubRoles = [JobSubRoleEnum.Healer],
			Jobs = [JobEnum.WhiteMage, JobEnum.Paladin],
		});

		Assert.Empty(await Matching(new MentorRouletteLogGridRequest
		{
			SubRoles = [JobSubRoleEnum.Tank],
			Jobs = [JobEnum.Sage],
		}));
	}

	[Theory]
	[InlineData(true, new[] { 4, 3, 1 })]
	[InlineData(false, new[] { 5, 2 })]
	public async Task Completed_filter_keeps_only_runs_with_that_outcome(bool completed, int[] expected)
	{
		await AssertMatches(expected, new MentorRouletteLogGridRequest { Completed = completed });
	}

	[Theory]
	[InlineData(true, new[] { 3, 2 })]
	[InlineData(false, new[] { 5, 4, 1 })]
	public async Task Replacement_filter_keeps_only_runs_with_that_replacement_flag(bool replacement, int[] expected)
	{
		await AssertMatches(expected, new MentorRouletteLogGridRequest { Replacement = replacement });
	}

	[Fact]
	public async Task Played_range_includes_its_start_and_excludes_its_end()
	{
		// All of March: run 2 is exactly on the start, run 5 is exactly on the end
		await AssertMatches([4, 3, 2], new MentorRouletteLogGridRequest
		{
			PlayedFrom = Utc(3, 1),
			PlayedBefore = Utc(4, 1),
		});
	}

	[Fact]
	public async Task Played_range_can_be_open_ended()
	{
		await AssertMatches([5, 4, 3], new MentorRouletteLogGridRequest { PlayedFrom = Utc(3, 15) });
		await AssertMatches([1], new MentorRouletteLogGridRequest { PlayedBefore = Utc(3, 1) });
	}

	[Fact]
	public async Task Played_range_bounds_are_compared_as_instants_not_wall_clock_times()
	{
		// Local midnight on March 1st in UTC-5 is 05:00 UTC, which is after run 2 (00:00 UTC)
		var localMidnight = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(-5));

		await AssertMatches([5, 4, 3], new MentorRouletteLogGridRequest { PlayedFrom = localMidnight });
	}

	[Fact]
	public async Task Filters_combine_with_each_other_and_with_search()
	{
		await AssertMatches([3], new MentorRouletteLogGridRequest
		{
			Completed = true,
			Replacement = true,
		});

		// "the" matches The Bowl of Embers and The Tower of Zot, but only one of them is from A Realm Reborn
		await AssertMatches([2], new MentorRouletteLogGridRequest
		{
			Search = "the",
			Expansions = [ExpansionEnum.ARealmReborn],
		});
	}

	[Fact]
	public async Task Filtered_total_counts_every_match_not_just_the_page()
	{
		var result = await CreateService().GetPageAsync(new MentorRouletteLogGridRequest { Completed = true, PageSize = 1, Page = 2 });

		Assert.True(result.IsSuccess, result.Error?.Message);
		Assert.Equal([3], result.Value.Items.Select(l => l.SortOrder));
		Assert.Equal(3, result.Value.TotalCount);
	}

	[Fact]
	public async Task Invalid_filters_are_rejected_before_querying()
	{
		var result = await CreateService().GetPageAsync(new MentorRouletteLogGridRequest { PlayedFrom = Utc(4, 1), PlayedBefore = Utc(3, 1) });

		Assert.Equal(ServiceErrorKind.Invalid, result.Error?.Kind);
		Assert.Equal("The played-from date must be before the played-before date.", result.Error?.Message);
	}
}
