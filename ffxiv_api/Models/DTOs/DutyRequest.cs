using ffxiv_api.Data.Configurations;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Body for creating or updating a duty. Only these fields are writable. Anything else the client
/// sends (dutyId, labels, ...) is ignored.
/// </summary>
public sealed record DutyRequest
{
	public string? Name { get; init; }

	public DutyTypeEnum? DutyType { get; init; }

	public ExpansionEnum? Expansion { get; init; }

	public long LevelRequirement { get; init; }

	public string NormalizedName => Name?.Trim() ?? string.Empty;

	/// <summary>
	/// Rules that don't need the database. Returns the first problem found, or null if valid.
	/// </summary>
	public string? Validate()
	{
		if (NormalizedName.Length == 0)
		{
			return "Name cannot be empty.";
		}

		if (NormalizedName.Length > DutyConfiguration.NameMaxLength)
		{
			return $"Name cannot be longer than {DutyConfiguration.NameMaxLength} characters.";
		}

		if (DutyType is not { } dutyType || !Enum.IsDefined(dutyType))
		{
			return "A valid duty type must be specified.";
		}

		if (Expansion is not { } expansion || expansion.GetLevelRange() is not var (min, max))
		{
			return "A valid expansion must be specified.";
		}

		if (LevelRequirement < min || LevelRequirement > max)
		{
			return $"The level requirement must be between {min} and {max} for {expansion.GetLabel()}.";
		}

		return null;
	}
}
