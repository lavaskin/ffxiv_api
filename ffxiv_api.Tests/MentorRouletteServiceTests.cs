using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using ffxiv_api.Services;
using Microsoft.EntityFrameworkCore;
using static ffxiv_api.Tests.TestData;

namespace ffxiv_api.Tests;

public sealed class MentorRouletteServiceTests : IDisposable
{
	private static readonly DateTime Now = new(2026, 9, 26, 18, 30, 0, DateTimeKind.Utc);
	private static readonly DateTime Earlier = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private readonly TestDatabase _db = new();
	private readonly MentorRouletteStatsCache _statsCache = new();
	private readonly Duty _sastasha = Duty("Sastasha", DutyTypeEnum.Dungeon);
	private readonly Duty _ifrit = Duty("The Bowl of Embers (Extreme)", DutyTypeEnum.ExtremeTrial);

	public MentorRouletteServiceTests()
	{
		_db.Seed(_sastasha, _ifrit);
	}

	public void Dispose() => _db.Dispose();

	private MentorRouletteService CreateService() => new(_db.CreateContext(), new FixedTimeProvider(Now), _statsCache);

	private static MentorRouletteLogRequest Request(Duty duty, bool completed = true) => new()
	{
		DutyId = duty.DutyId,
		PlayedJob = JobEnum.Paladin,
		Completed = completed,
	};

	private MentorRouletteLog Log(int sortOrder, Duty? duty = null, JobEnum job = JobEnum.Paladin, bool completed = true, string notes = "") => new()
	{
		DutyId = (duty ?? _sastasha).DutyId,
		SortOrder = sortOrder,
		PlayedJob = job,
		Completed = completed,
		Notes = notes,
		DatePlayed = Earlier,
	};

	private async Task<PagedResponse<MentorRouletteLogResponse>> Page(MentorRouletteLogGridRequest request)
	{
		var result = await CreateService().GetPageAsync(request);
		Assert.True(result.IsSuccess, result.Error?.Message);
		return result.Value;
	}

	[Fact]
	public async Task Page_defaults_to_newest_run_first_with_their_duty()
	{
		_db.Seed(Log(1), Log(3, _ifrit), Log(2));

		var page = await Page(new MentorRouletteLogGridRequest());

		Assert.Equal([3, 2, 1], page.Items.Select(l => l.SortOrder));
		Assert.Equal("The Bowl of Embers (Extreme)", page.Items[0].Duty?.Name);
		Assert.Equal("Extreme Trial", page.Items[0].Duty?.DutyTypeLabel);
		Assert.Equal("Paladin", page.Items[0].PlayedJobLabel);
		Assert.Equal(3, page.TotalCount);
	}

	/// <summary>
	/// The same fields the roulettes grid used to filter on in the browser, including the labels it shows
	/// </summary>
	[Theory]
	[InlineData("white", new[] { 2 })]
	[InlineData("SAGE", new[] { 3 })]
	[InlineData("sasta", new[] { 3, 1 })]
	[InlineData("extreme trial", new[] { 2 })]
	[InlineData("dungeon", new[] { 3, 1 })]
	[InlineData("Wiped", new[] { 3 })]
	[InlineData("no such run", new int[0])]
	public async Task Search_matches_job_duty_name_duty_type_and_notes_ignoring_case(string search, int[] expected)
	{
		_db.Seed(
			Log(1, _sastasha, JobEnum.Paladin),
			Log(2, _ifrit, JobEnum.WhiteMage),
			Log(3, _sastasha, JobEnum.Sage, notes: "wiped twice on the boss"));

		var page = await Page(new MentorRouletteLogGridRequest { Search = search });

		Assert.Equal(expected, page.Items.Select(l => l.SortOrder));
		Assert.Equal(expected.Length, page.TotalCount);
	}

	[Fact]
	public async Task Sorting_by_duty_name_keeps_newest_first_within_a_duty()
	{
		_db.Seed(Log(1, _sastasha), Log(2, _ifrit), Log(3, _sastasha));

		var page = await Page(new MentorRouletteLogGridRequest { SortBy = "dutyName" });

		// "Sastasha" sorts before "The Bowl of Embers (Extreme)"
		Assert.Equal([3, 1, 2], page.Items.Select(l => l.SortOrder));
	}

	[Fact]
	public async Task Played_job_sorts_in_role_order_not_by_label()
	{
		_db.Seed(Log(1, job: JobEnum.Sage), Log(2, job: JobEnum.WhiteMage), Log(3, job: JobEnum.Paladin));

		var page = await Page(new MentorRouletteLogGridRequest { SortBy = "playedJob" });

		// By label this would be Paladin, Sage, White Mage
		Assert.Equal(["Paladin", "White Mage", "Sage"], page.Items.Select(l => l.PlayedJobLabel));
	}

	/// <summary>
	/// The keys the roulettes grid sends as <c>sortBy</c>
	/// </summary>
	[Theory]
	[InlineData("sortOrder")]
	[InlineData("playedJob")]
	[InlineData("dutyName")]
	[InlineData("dutyType")]
	[InlineData("completed")]
	[InlineData("replacement")]
	[InlineData("notes")]
	[InlineData("datePlayed")]
	public async Task Accepts_every_sort_key_the_client_sends(string sortBy)
	{
		_db.Seed(Log(1), Log(2, _ifrit));

		var result = await CreateService().GetPageAsync(new MentorRouletteLogGridRequest { SortBy = sortBy, SortDirection = SortDirection.Desc });

		Assert.True(result.IsSuccess, result.Error?.Message);
		Assert.Equal(2, result.Value.Items.Count);
	}

	[Fact]
	public async Task Create_numbers_the_run_and_stamps_the_server_time()
	{
		_db.Seed(Log(1), Log(2));

		var result = await CreateService().CreateAsync(new MentorRouletteLogRequest
		{
			DutyId = _ifrit.DutyId,
			PlayedJob = JobEnum.Sage,
			Completed = false,
			Notes = null,
		});

		Assert.True(result.IsSuccess);
		Assert.Equal(3, result.Value.SortOrder);
		Assert.Equal(Now, result.Value.DatePlayed);
		Assert.Equal("The Bowl of Embers (Extreme)", result.Value.Duty?.Name);

		await using var context = _db.CreateContext();
		var saved = await context.MentorRouletteLogs.SingleAsync(l => l.SortOrder == 3);
		Assert.Equal(JobEnum.Sage, saved.PlayedJob);
		Assert.False(saved.Completed);
		Assert.Equal(string.Empty, saved.Notes);
		Assert.Equal(2, await context.Duties.CountAsync()); // the duty wasn't re-inserted
	}

	[Fact]
	public async Task The_first_run_is_number_one()
	{
		var result = await CreateService().CreateAsync(new MentorRouletteLogRequest { DutyId = _sastasha.DutyId, PlayedJob = JobEnum.Paladin });

		Assert.Equal(1, result.Value?.SortOrder);
	}

	[Fact]
	public async Task Create_rejects_a_duty_that_does_not_exist()
	{
		var result = await CreateService().CreateAsync(new MentorRouletteLogRequest { DutyId = 12345, PlayedJob = JobEnum.Paladin });

		Assert.Equal(ServiceErrorKind.Invalid, result.Error?.Kind);
		await using var context = _db.CreateContext();
		Assert.False(await context.MentorRouletteLogs.AnyAsync());
	}

	[Fact]
	public async Task Update_changes_writable_fields_but_keeps_run_number_and_date()
	{
		var log = Log(5);
		_db.Seed(log);

		var result = await CreateService().UpdateAsync(log.MentorRouletteLogId, new MentorRouletteLogRequest
		{
			DutyId = _ifrit.DutyId,
			PlayedJob = JobEnum.WhiteMage,
			Completed = false,
			Replacement = true,
			Notes = "wiped to the add phase",
		});

		Assert.True(result.IsSuccess);
		Assert.Equal("The Bowl of Embers (Extreme)", result.Value.Duty?.Name);

		await using var context = _db.CreateContext();
		var saved = await context.MentorRouletteLogs.SingleAsync();
		Assert.Equal(_ifrit.DutyId, saved.DutyId);
		Assert.Equal(JobEnum.WhiteMage, saved.PlayedJob);
		Assert.False(saved.Completed);
		Assert.True(saved.Replacement);
		Assert.Equal("wiped to the add phase", saved.Notes);
		Assert.Equal(5, saved.SortOrder);
		Assert.Equal(Earlier, saved.DatePlayed);
	}

	[Fact]
	public async Task Update_returns_not_found_for_a_missing_log()
	{
		var result = await CreateService().UpdateAsync(12345, new MentorRouletteLogRequest { DutyId = _sastasha.DutyId, PlayedJob = JobEnum.Paladin });

		Assert.Equal(ServiceErrorKind.NotFound, result.Error?.Kind);
	}

	[Fact]
	public async Task Delete_removes_the_log()
	{
		var log = Log(1);
		_db.Seed(log);

		var error = await CreateService().DeleteAsync(log.MentorRouletteLogId);

		Assert.Null(error);
		await using var context = _db.CreateContext();
		Assert.False(await context.MentorRouletteLogs.AnyAsync());
	}

	[Fact]
	public async Task Delete_returns_not_found_for_a_missing_log()
	{
		var error = await CreateService().DeleteAsync(12345);

		Assert.Equal(ServiceErrorKind.NotFound, error?.Kind);
	}

	[Fact]
	public async Task Stats_are_calculated_from_the_stored_runs()
	{
		_db.Seed(
			Log(1, _ifrit, JobEnum.Warrior, completed: true),
			Log(2, _ifrit, JobEnum.Warrior, completed: false),
			Log(3, _sastasha, JobEnum.Sage));

		var stats = await CreateService().GetStatsAsync();

		Assert.Equal(3, stats.TotalRuns);
		Assert.Equal(1, stats.TotalFailedDuties);
		Assert.Equal(2, stats.NumberExtremeTrials);
		Assert.Equal(50, stats.ExtremeTrialClearPercent);
		Assert.Equal("The Bowl of Embers (Extreme)", stats.TopSeenDuties[0].DutyName);
		Assert.Equal("Warrior", stats.TopPlayedJobs[0].JobLabel);
	}

	[Fact]
	public async Task Stats_are_cached_between_calls()
	{
		_db.Seed(Log(1));
		var first = await CreateService().GetStatsAsync();

		_db.Seed(Log(2)); // written behind the API's back, so the cache doesn't know

		var second = await CreateService().GetStatsAsync();
		Assert.Same(first, second);
		Assert.Equal(1, second.TotalRuns);
	}

	[Fact]
	public async Task Creating_a_log_refreshes_the_stats()
	{
		var before = await CreateService().GetStatsAsync();

		await CreateService().CreateAsync(Request(_sastasha));

		var after = await CreateService().GetStatsAsync();
		Assert.Equal(0, before.TotalRuns);
		Assert.Equal(1, after.TotalRuns);
	}

	[Fact]
	public async Task Updating_a_log_refreshes_the_stats()
	{
		var log = Log(1, completed: true);
		_db.Seed(log);
		var before = await CreateService().GetStatsAsync();

		await CreateService().UpdateAsync(log.MentorRouletteLogId, Request(_sastasha, completed: false));

		var after = await CreateService().GetStatsAsync();
		Assert.Equal(0, before.TotalFailedDuties);
		Assert.Equal(1, after.TotalFailedDuties);
	}

	[Fact]
	public async Task Deleting_a_log_refreshes_the_stats()
	{
		var log = Log(1);
		_db.Seed(log);
		var before = await CreateService().GetStatsAsync();

		await CreateService().DeleteAsync(log.MentorRouletteLogId);

		var after = await CreateService().GetStatsAsync();
		Assert.Equal(1, before.TotalRuns);
		Assert.Equal(0, after.TotalRuns);
	}

	[Fact]
	public async Task A_rejected_write_keeps_the_cached_stats()
	{
		var before = await CreateService().GetStatsAsync();

		await CreateService().CreateAsync(new MentorRouletteLogRequest { DutyId = 12345, PlayedJob = JobEnum.Paladin });

		Assert.Same(before, await CreateService().GetStatsAsync());
	}
}
