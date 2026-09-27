using ffxiv_api.Models.DTOs;

namespace ffxiv_api.Services;

/// <summary>
/// Holds the last calculated stats until something that feeds them changes. Registered as a
/// singleton. Anything that writes logs, or duty fields shown in the stats, must call
/// <see cref="Invalidate"/> after saving.
///
/// Deliberately simple: no expiry and no protection against a stats read racing a write. This is
/// fine for a single local user. Edits made outside the API (e.g. directly in SQL) show up after
/// the next write or a restart.
/// </summary>
public class MentorRouletteStatsCache
{
	private MentorRouletteStats? _stats;

	public async Task<MentorRouletteStats> GetOrCreateAsync(Func<Task<MentorRouletteStats>> calculate)
	{
		return _stats ??= await calculate();
	}

	public void Invalidate() => _stats = null;
}
