namespace ffxiv_api.Models.DTOs;

public class MentorRouletteStats
{
	public int TotalRuns { get; set; }

	public int CompletedRoulettes { get; set; }
	
	/// <summary>
	/// Completed Roulettes / 2000 Completions needed for the achievement
	/// </summary>
	public int AchievementProgressPercent { get; set; }

	public List<SeenDutyStat> TopSeenDuties { get; set; } = [];

	public List<PlayedJobStat> TopPlayedJobs { get; set; } = [];

	public List<PlayedJobDutyBreakdownStat> PlayedJobDutyTypeBreakdown { get; set; } = [];

	public int TotalFailedDuties { get; set; }

	public int NumberExtremeTrials { get; set; }

	public int ExtremeTrialClearPercent { get; set; }

	public List<DutyExpansionBreakdownStat> DutyExpansionBreakdown { get; set; } = [];
}
