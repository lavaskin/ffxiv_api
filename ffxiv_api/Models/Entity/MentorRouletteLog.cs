using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;

public class MentorRouletteLog
{
	// Navigation Properties
	public DutyModel? DutyModel { get; set; }	
	
	/// <summary>
	/// Primary Key
	/// </summary>
	[Key]
	public int MentorRouletteLogId { get; set; }

	[ForeignKey("DutyModel")]
	public int DutyId { get; set; }
	
	public int SortOrder { get; set; }

	public int PlayedJobId { get; set; }

	public string Notes { get; set; } = string.Empty;

	public DateTime DatePlayed { get; set; }

	#region NotMapped

	[NotMapped]
	public JobEnum? PlayedJob
	{
		get => (JobEnum)PlayedJobId;
		set => PlayedJobId = value.HasValue ? (int)value.Value : 0;
	}

	[NotMapped]
	public string? PlayedJobLabel { get; set; }

	public void SetNotMapped()
	{
		// Set NotMapped properties here
	}

	#endregion
}
