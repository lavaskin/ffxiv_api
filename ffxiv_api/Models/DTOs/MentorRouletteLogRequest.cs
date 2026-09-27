using System.Text.Json.Serialization;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Body for creating or updating a log. Only these fields are writable. SortOrder and DatePlayed
/// are assigned by the server on creation, and anything else the client sends (sortOrder,
/// datePlayed, playedJob, labels, ...) is ignored.
/// </summary>
public sealed record MentorRouletteLogRequest
{
	public long DutyId { get; init; }

	[JsonPropertyName("playedJobId")]
	public JobEnum? PlayedJob { get; init; }

	public bool Completed { get; init; } = true;

	public bool Replacement { get; init; }

	public string? Notes { get; init; }

	/// <summary>
	/// Rules that don't need the database. Returns the first problem found, or null if valid.
	/// </summary>
	public string? Validate()
	{
		if (DutyId <= 0)
		{
			return "Must have an associated duty";
		}

		if (PlayedJob is not { } playedJob || !Enum.IsDefined(playedJob))
		{
			return "Must have a valid played job";
		}

		return null;
	}
}
