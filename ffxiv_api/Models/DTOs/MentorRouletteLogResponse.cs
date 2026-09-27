using System.Text.Json.Serialization;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.DTOs;

public sealed record MentorRouletteLogResponse
{
	public long MentorRouletteLogId { get; init; }

	public long DutyId { get; init; }

	[JsonPropertyName("dutyModel")]
	public DutyResponse? Duty { get; init; }

	public int SortOrder { get; init; }

	[JsonPropertyName("playedJobId")]
	public JobEnum PlayedJob { get; init; }

	public string PlayedJobLabel { get; init; } = string.Empty;

	public bool Completed { get; init; }

	public bool Replacement { get; init; }

	public string Notes { get; init; } = string.Empty;

	public DateTime DatePlayed { get; init; }

	/// <summary>
	/// Expects <see cref="MentorRouletteLog.Duty"/> to be loaded. If it isn't, <see cref="Duty"/> is null.
	/// </summary>
	public static MentorRouletteLogResponse FromEntity(MentorRouletteLog log)
	{
		return new MentorRouletteLogResponse
		{
			MentorRouletteLogId = log.MentorRouletteLogId,
			DutyId = log.DutyId,
			Duty = log.Duty is null ? null : DutyResponse.FromEntity(log.Duty),
			SortOrder = log.SortOrder,
			PlayedJob = log.PlayedJob,
			PlayedJobLabel = log.PlayedJob.GetLabel(),
			Completed = log.Completed,
			Replacement = log.Replacement,
			Notes = log.Notes,
			DatePlayed = log.DatePlayed,
		};
	}
}
