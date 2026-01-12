using ffxiv_api.Data;

namespace ffxiv_api.Services;

public class MentorRouletteService
{
	public int GetNextSortOrder(AppDbContext context)
	{
		try
		{
			var maxSortOrder = context.MentorRouletteLogs.Max(log => (int?)log.SortOrder) ?? 0;
			return maxSortOrder + 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error getting next sort order: {ex.Message}");
			return 1;
		}
	}
}
