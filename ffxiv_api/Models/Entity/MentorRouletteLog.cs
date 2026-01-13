using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.Entity;

public class MentorRouletteLogModel : BaseModel
{
	// Navigation Properties
	public DutyModel? DutyModel { get; set; } = null;
	
	/// <summary>
	/// Primary Key
	/// </summary>
	[Key]
	public long MentorRouletteLogId { get; set; }

	[ForeignKey("DutyModel")]
	public long DutyId { get; set; }
	
	public int SortOrder { get; set; }

	public long? PlayedJobId { get; set; }

	public string Notes { get; set; } = string.Empty;

	public DateTime DatePlayed { get; set; }

	#region NotMapped

	[NotMapped]
	public JobEnum? PlayedJob
	{
		get => PlayedJobId.HasValue ? (JobEnum?)PlayedJobId.Value : null;
		set => PlayedJobId = value.HasValue ? (long?)value.Value : null;
	}

	[NotMapped]
	public string? PlayedJobLabel { get; set; }

	public void SetNotMapped()
	{
		if (PlayedJob.HasValue)
		{
			PlayedJobLabel = PlayedJob.Value.GetLabel();
		}

		if (DutyModel != null)
		{
			DutyModel.SetNotMapped();
		}
	}

	#endregion
	
	public override string? Validate()
	{
		if (DutyId <= 0)
		{
			return "Must have an associated duty";
		}
		
		if (!PlayedJobId.HasValue || PlayedJobId <= 0)
		{
			return "Must have a valid played job";
		}

		return null;
	}
}
