using ffxiv_api.Models.Enums;
using ffxiv_api.Services;

namespace ffxiv_api.Tests;

public class MentorRouletteStatsCalculatorTests
{
	private static RouletteRun Run(
		string dutyName = "Sastasha",
		DutyTypeEnum? dutyType = DutyTypeEnum.Dungeon,
		ExpansionEnum? expansion = ExpansionEnum.ARealmReborn,
		JobEnum job = JobEnum.Paladin,
		bool completed = true)
	{
		return new RouletteRun(dutyName, dutyType, expansion, job, completed);
	}

	private static List<RouletteRun> Repeat(int count, RouletteRun run) => Enumerable.Repeat(run, count).ToList();

	[Fact]
	public void No_runs_produces_zeroes_without_dividing_by_zero()
	{
		var stats = MentorRouletteStatsCalculator.Calculate([]);

		Assert.Equal(0, stats.TotalRuns);
		Assert.Equal(0, stats.AchievementProgressPercent);
		Assert.Equal(0, stats.ExtremeTrialClearPercent);
		Assert.Empty(stats.TopSeenDuties);
		Assert.Empty(stats.TopPlayedJobs);
		Assert.Empty(stats.PlayedJobDutyTypeBreakdown);
		Assert.Empty(stats.DutyTypeRoleBreakdown);
		Assert.Empty(stats.DutyExpansionBreakdown);
	}

	[Fact]
	public void Counts_completed_and_failed_runs()
	{
		var runs = Repeat(7, Run(completed: true)).Concat(Repeat(3, Run(completed: false))).ToList();

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		Assert.Equal(10, stats.TotalRuns);
		Assert.Equal(7, stats.CompletedRoulettes);
		Assert.Equal(3, stats.TotalFailedDuties);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(30, 2)] // 1.5% rounds to 2
	[InlineData(1000, 50)]
	[InlineData(2000, 100)]
	[InlineData(2500, 100)] // clamped
	public void Achievement_progress_is_a_clamped_percentage_of_the_target(int completed, int expectedPercent)
	{
		var stats = MentorRouletteStatsCalculator.Calculate(Repeat(completed, Run()));

		Assert.Equal(expectedPercent, stats.AchievementProgressPercent);
	}

	[Fact]
	public void Extreme_trial_clear_percent_only_considers_extreme_trials()
	{
		var runs = new List<RouletteRun>
		{
			Run(dutyType: DutyTypeEnum.ExtremeTrial, completed: true),
			Run(dutyType: DutyTypeEnum.ExtremeTrial, completed: true),
			Run(dutyType: DutyTypeEnum.ExtremeTrial, completed: false),
			Run(dutyType: DutyTypeEnum.Dungeon, completed: false),
		};

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		Assert.Equal(3, stats.NumberExtremeTrials);
		Assert.Equal(67, stats.ExtremeTrialClearPercent);
	}

	[Fact]
	public void Top_seen_duties_are_the_three_most_run_with_ties_broken_alphabetically()
	{
		var runs = Repeat(3, Run("Haukke Manor"))
			.Concat(Repeat(2, Run("Sastasha")))
			.Concat(Repeat(2, Run("Copperbell Mines")))
			.Concat(Repeat(2, Run("Halatali")))
			.ToList();

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		Assert.Equal(
			[("Haukke Manor", 3), ("Copperbell Mines", 2), ("Halatali", 2)],
			stats.TopSeenDuties.Select(s => (s.DutyName, s.Count)));
	}

	[Fact]
	public void Top_played_jobs_ignore_unknown_job_ids_and_break_ties_by_job_id()
	{
		var runs = Repeat(5, Run(job: (JobEnum)999))
			.Concat(Repeat(2, Run(job: JobEnum.Sage)))
			.Concat(Repeat(2, Run(job: JobEnum.Warrior)))
			.Concat(Repeat(1, Run(job: JobEnum.Viper)))
			.Concat(Repeat(1, Run(job: JobEnum.Paladin)))
			.ToList();

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		Assert.Equal(
			[("Warrior", 2), ("Sage", 2), ("Paladin", 1)],
			stats.TopPlayedJobs.Select(s => (s.JobLabel, s.Count)));
	}

	[Fact]
	public void Played_job_breakdown_includes_guildhests_and_omits_empty_duty_types()
	{
		var runs = new List<RouletteRun>
		{
			Run(job: JobEnum.Paladin, dutyType: DutyTypeEnum.Guildhest),
			Run(job: JobEnum.Paladin, dutyType: DutyTypeEnum.Dungeon),
			Run(job: JobEnum.Paladin, dutyType: DutyTypeEnum.Dungeon),
			Run(job: JobEnum.Paladin, dutyType: null), // no duty type: excluded
		};

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		var paladin = Assert.Single(stats.PlayedJobDutyTypeBreakdown);
		Assert.Equal("Paladin", paladin.JobLabel);
		Assert.Equal(
			[("Dungeon", 2), ("Guildhest", 1)],
			paladin.DutyTypes.Select(d => (d.DutyTypeLabel, d.Count)));
	}

	[Fact]
	public void Duty_type_role_breakdown_groups_jobs_into_roles_most_frequent_duty_type_first()
	{
		var runs = new List<RouletteRun>
		{
			Run(dutyType: DutyTypeEnum.Trial, job: JobEnum.WhiteMage),
			Run(dutyType: DutyTypeEnum.Dungeon, job: JobEnum.Paladin),
			Run(dutyType: DutyTypeEnum.Dungeon, job: JobEnum.Warrior),
			Run(dutyType: DutyTypeEnum.Dungeon, job: JobEnum.Bard),
			Run(dutyType: DutyTypeEnum.Guildhest, job: JobEnum.Monk),
			Run(dutyType: (DutyTypeEnum)99, job: JobEnum.Monk), // unknown duty type: excluded
		};

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		Assert.Equal(["Dungeon", "Trial", "Guildhest"], stats.DutyTypeRoleBreakdown.Select(d => d.DutyTypeLabel));

		var dungeon = stats.DutyTypeRoleBreakdown[0];
		Assert.Equal(3, dungeon.Count);
		Assert.Equal([("Tank", 2), ("DPS", 1)], dungeon.Roles.Select(r => (r.RoleLabel, r.Count)));
	}

	[Fact]
	public void Expansion_breakdown_excludes_guildhests_and_is_ordered_by_expansion()
	{
		var runs = new List<RouletteRun>
		{
			Run(expansion: ExpansionEnum.Dawntrail, dutyType: DutyTypeEnum.NormalRaid),
			Run(expansion: ExpansionEnum.ARealmReborn, dutyType: DutyTypeEnum.Dungeon),
			Run(expansion: ExpansionEnum.ARealmReborn, dutyType: DutyTypeEnum.Guildhest),
			Run(expansion: ExpansionEnum.Heavensward, dutyType: DutyTypeEnum.Guildhest), // only guildhests: no entry
			Run(expansion: null, dutyType: DutyTypeEnum.Dungeon), // no expansion: excluded
		};

		var stats = MentorRouletteStatsCalculator.Calculate(runs);

		Assert.Equal(
			["2.0: A Realm Reborn", "7.0: Dawntrail"],
			stats.DutyExpansionBreakdown.Select(e => e.ExpansionLabel));
		Assert.Equal(
			[("Dungeon", 1)],
			stats.DutyExpansionBreakdown[0].DutyTypes.Select(d => (d.DutyTypeLabel, d.Count)));
	}
}
