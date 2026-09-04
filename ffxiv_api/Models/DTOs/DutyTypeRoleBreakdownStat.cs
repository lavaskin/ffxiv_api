namespace ffxiv_api.Models.DTOs;

public class DutyTypeRoleBreakdownStat
{
	public string DutyTypeLabel { get; set; } = string.Empty;

	public List<JobRoleBreakdownStat> Roles { get; set; } = [];

	/// <summary>
	/// Total number of duties of this type across every role. Used to sort the chart by frequency.
	/// </summary>
	public int Count { get; set; }
}
