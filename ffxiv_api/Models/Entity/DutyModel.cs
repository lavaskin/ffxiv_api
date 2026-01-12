using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.Entity;

public class DutyModel
{	
	
	/// <summary>
	/// Primary Key
	/// </summary>
	[Key]
	public int DutyId { get; set; }

	public string Name { get; set; } = string.Empty;

	public int? DutyTypeId { get; set; }

	public int? ExpansionId { get; set; }
	
	public int LevelRequirement { get; set; }

	#region NotMapped

	[NotMapped]
	public DutyTypeEnum? DutyType
	{
		get => DutyTypeId.HasValue ? (DutyTypeEnum?)DutyTypeId.Value : null;
		set => DutyTypeId = value.HasValue ? (int?)value.Value : null;
	}

	[NotMapped]
	public string? DutyTypeLabel { get; set;}

	[NotMapped]
	public ExpansionEnum? Expansion
	{
		get => ExpansionId.HasValue ? (ExpansionEnum?)ExpansionId.Value : null;
		set => ExpansionId = value.HasValue ? (int?)value.Value : null;
	}
	
	[NotMapped]
	public string? ExpansionLabel { get; set; }

	public void SetNotMapped()
	{
		if (DutyType.HasValue)
		{
			DutyTypeLabel = DutyType.Value.GetLabel();
		}

		if (Expansion.HasValue)
		{
			ExpansionLabel = Expansion.Value.GetLabel();
		}
	}

	#endregion
}
