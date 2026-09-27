using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;
using ffxiv_api.Services;

namespace ffxiv_api.Models.DTOs;

public sealed record DutyResponse
{
	public long DutyId { get; init; }

	public string Name { get; init; } = string.Empty;

	public DutyTypeEnum? DutyType { get; init; }

	public string? DutyTypeLabel { get; init; }

	public ExpansionEnum? Expansion { get; init; }

	public string? ExpansionLabel { get; init; }

	public long LevelRequirement { get; init; }

	public DutyClassificationEnum? DutyClassification { get; init; }

	public string? DutyClassificationLabel { get; init; }

	public static DutyResponse FromEntity(Duty duty)
	{
		var classification = DutyClassifier.Classify(duty);

		return new DutyResponse
		{
			DutyId = duty.DutyId,
			Name = duty.Name,
			DutyType = duty.DutyType,
			DutyTypeLabel = duty.DutyType?.GetLabel(),
			Expansion = duty.Expansion,
			ExpansionLabel = duty.Expansion?.GetLabel(),
			LevelRequirement = duty.LevelRequirement,
			DutyClassification = classification,
			DutyClassificationLabel = classification?.GetLabel(),
		};
	}
}
