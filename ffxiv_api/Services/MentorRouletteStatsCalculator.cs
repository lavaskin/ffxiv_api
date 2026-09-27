using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Services;

/// <summary>
/// The slice of a mentor roulette log that the stats need.
/// </summary>
public sealed record RouletteRun(
	string DutyName,
	DutyTypeEnum? DutyType,
	ExpansionEnum? Expansion,
	JobEnum PlayedJob,
	bool Completed
);

/// <summary>
/// Turns runs into homepage stats. Pure, with no database access, so it can be unit tested directly.
/// </summary>
public static class MentorRouletteStatsCalculator
{
	/// <summary>
	/// Completed roulettes needed for the mentor roulette achievement
	/// </summary>
	public const int AchievementTarget = 2000;

	private const int TopListSize = 3;

	private static readonly DutyTypeEnum[] AllDutyTypes = Enum.GetValues<DutyTypeEnum>();

	private static readonly JobRoleEnum[] AllJobRoles = Enum.GetValues<JobRoleEnum>();

	public static MentorRouletteStats Calculate(IReadOnlyCollection<RouletteRun> runs)
	{
		int completed = runs.Count(run => run.Completed);
		var extremeTrials = runs.Where(run => run.DutyType == DutyTypeEnum.ExtremeTrial).ToList();

		return new MentorRouletteStats
		{
			TotalRuns = runs.Count,
			CompletedRoulettes = completed,
			TotalFailedDuties = runs.Count - completed,
			AchievementProgressPercent = Math.Clamp(Percent(completed, AchievementTarget), 0, 100),
			NumberExtremeTrials = extremeTrials.Count,
			ExtremeTrialClearPercent = Percent(extremeTrials.Count(run => run.Completed), extremeTrials.Count),
			TopSeenDuties = TopSeenDuties(runs),
			TopPlayedJobs = TopPlayedJobs(runs),
			PlayedJobDutyTypeBreakdown = PlayedJobDutyTypeBreakdown(runs),
			DutyTypeRoleBreakdown = DutyTypeRoleBreakdown(runs),
			DutyExpansionBreakdown = DutyExpansionBreakdown(runs),
		};
	}

	private static List<SeenDutyStat> TopSeenDuties(IEnumerable<RouletteRun> runs)
	{
		return runs
			.GroupBy(run => run.DutyName)
			.OrderByDescending(group => group.Count())
			.ThenBy(group => group.Key)
			.Take(TopListSize)
			.Select(group => new SeenDutyStat
			{
				DutyName = group.Key,
				Count = group.Count(),
			})
			.ToList();
	}

	private static List<PlayedJobStat> TopPlayedJobs(IEnumerable<RouletteRun> runs)
	{
		return GroupByKnownJob(runs)
			.Take(TopListSize)
			.Select(group => new PlayedJobStat
			{
				JobLabel = group.Key.GetLabel(),
				Count = group.Count(),
			})
			.ToList();
	}

	/// <summary>
	/// Per job, how many of each duty type it was played in. Guildhests are included here.
	/// </summary>
	private static List<PlayedJobDutyBreakdownStat> PlayedJobDutyTypeBreakdown(IEnumerable<RouletteRun> runs)
	{
		var runsWithDutyType = runs.Where(run => run.DutyType.HasValue);

		return GroupByKnownJob(runsWithDutyType)
			.Select(group => new PlayedJobDutyBreakdownStat
			{
				JobLabel = group.Key.GetLabel(),
				DutyTypes = CountByDutyType(group, AllDutyTypes),
			})
			.ToList();
	}

	/// <summary>
	/// Per duty type, how many runs were played as each role. Guildhests are included here,
	/// matching <see cref="PlayedJobDutyTypeBreakdown"/>.
	/// </summary>
	private static List<DutyTypeRoleBreakdownStat> DutyTypeRoleBreakdown(IEnumerable<RouletteRun> runs)
	{
		return runs
			.Where(run =>
				Enum.IsDefined(run.PlayedJob) &&
				run.DutyType is { } dutyType &&
				Enum.IsDefined(dutyType))
			.GroupBy(run => run.DutyType!.Value)
			.OrderByDescending(group => group.Count())
			.ThenBy(group => group.Key)
			.Select(group => new DutyTypeRoleBreakdownStat
			{
				DutyTypeLabel = group.Key.GetLabel(),
				Count = group.Count(),
				Roles = AllJobRoles
					.Select(role => new JobRoleBreakdownStat
					{
						RoleLabel = role.GetLabel(),
						Count = group.Count(run => run.PlayedJob.GetRole() == role),
					})
					.Where(stat => stat.Count > 0)
					.ToList(),
			})
			.ToList();
	}

	/// <summary>
	/// Per expansion, how many of each duty type were run. Guildhests are excluded because they
	/// pollute this chart.
	/// </summary>
	private static List<DutyExpansionBreakdownStat> DutyExpansionBreakdown(IEnumerable<RouletteRun> runs)
	{
		var dutyTypes = AllDutyTypes.Where(dutyType => dutyType != DutyTypeEnum.Guildhest).ToList();

		return runs
			.Where(run =>
				run.Expansion.HasValue &&
				run.DutyType.HasValue &&
				run.DutyType != DutyTypeEnum.Guildhest)
			.GroupBy(run => run.Expansion!.Value)
			.OrderBy(group => group.Key)
			.Select(group => new DutyExpansionBreakdownStat
			{
				ExpansionLabel = group.Key.GetLabel(),
				DutyTypes = CountByDutyType(group, dutyTypes),
			})
			.ToList();
	}

	/// <summary>
	/// Runs grouped by job (ignoring unknown job ids), most played first, ties broken by job id.
	/// </summary>
	private static IEnumerable<IGrouping<JobEnum, RouletteRun>> GroupByKnownJob(IEnumerable<RouletteRun> runs)
	{
		return runs
			.Where(run => Enum.IsDefined(run.PlayedJob))
			.GroupBy(run => run.PlayedJob)
			.OrderByDescending(group => group.Count())
			.ThenBy(group => group.Key);
	}

	/// <summary>
	/// Counts runs per duty type, in the order given, omitting duty types with no runs.
	/// </summary>
	private static List<DutyTypeBreakdownStat> CountByDutyType(IEnumerable<RouletteRun> runs, IEnumerable<DutyTypeEnum> dutyTypes)
	{
		return dutyTypes
			.Select(dutyType => new DutyTypeBreakdownStat
			{
				DutyTypeLabel = dutyType.GetLabel(),
				Count = runs.Count(run => run.DutyType == dutyType),
			})
			.Where(stat => stat.Count > 0)
			.ToList();
	}

	private static int Percent(int part, int whole)
	{
		return whole == 0 ? 0 : (int)Math.Round(part * 100.0 / whole);
	}
}
