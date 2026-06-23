namespace ffxiv_api.Models.DTOs;

public class DutyExpansionBreakdownStat
{
	public string ExpansionLabel { get; set; } = string.Empty;

	public List<DutyTypeBreakdownStat> DutyTypes { get; set; } = [];
}
