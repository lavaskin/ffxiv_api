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

	/// <summary>
	/// The expansion with the most related duties (excluding guildhests)
	/// </summary>
	public string MostCommonExpansion { get; set; } = string.Empty;

	public List<PlayedJobStat> TopPlayedJobs { get; set; } = [];

	public int TotalFailedDuties { get; set; }

	public int NumberExtremeTrials { get; set; }

	public int ExtremeTrialClearPercent { get; set; }
}
