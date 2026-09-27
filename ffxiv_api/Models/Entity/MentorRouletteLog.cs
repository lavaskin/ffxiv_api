using ffxiv_api.Models.Enums;

namespace ffxiv_api.Models.Entity;

/// <summary>
/// A row in the <c>mentor_roulette_log</c> table. Persistence only: never bind this from a request or
/// return it from an endpoint. Use <see cref="DTOs.MentorRouletteLogRequest"/> /
/// <see cref="DTOs.MentorRouletteLogResponse"/> instead.
/// Mapping lives in <see cref="Data.Configurations.MentorRouletteLogConfiguration"/>.
/// </summary>
public class MentorRouletteLog
{
	public long MentorRouletteLogId { get; set; }

	public long DutyId { get; set; }

	public Duty Duty { get; set; } = null!;

	/// <summary>
	/// The run number shown to the user (1, 2, 3...). Assigned on creation and never changed by updates.
	/// </summary>
	public int SortOrder { get; set; }

	public JobEnum PlayedJob { get; set; }

	public bool Completed { get; set; } = true;

	public bool Replacement { get; set; }

	public string Notes { get; set; } = string.Empty;

	/// <summary>
	/// UTC timestamp assigned on creation and never changed by updates.
	/// </summary>
	public DateTime DatePlayed { get; set; }
}
