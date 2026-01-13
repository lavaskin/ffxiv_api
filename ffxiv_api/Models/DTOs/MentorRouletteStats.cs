namespace ffxiv_api.Models.DTOs;

public class MentorRouletteStats
{
	public int TotalRuns { get; set; }

	public string MostRanDuty { get; set; } = string.Empty;

	public int MostRanDutyCount { get; set; }

	/// <summary>
	/// The expansion with the most related duties (excluding guildhests)
	/// </summary>
	public string MostCommonExpansion { get; set; } = string.Empty;

	public int TotalFailedDuties { get; set; }

	public int NumberExtremeTrials { get; set; }

	public int ExtremeTrialClearPercent { get; set; }
}
