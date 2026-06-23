namespace ffxiv_api.Models.DTOs;

public class PlayedJobDutyBreakdownStat
{
	public string JobLabel { get; set; } = string.Empty;

	public List<DutyTypeBreakdownStat> DutyTypes { get; set; } = [];
}
