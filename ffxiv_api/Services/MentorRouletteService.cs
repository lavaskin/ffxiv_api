using ffxiv_api.Data;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

public class MentorRouletteService
{
	public async Task<int> GetNextSortOrder(AppDbContext context)
	{
		try
		{
			var maxSortOrder = await context.MentorRouletteLogs.MaxAsync(log => (int?)log.SortOrder) ?? 0;
			return maxSortOrder + 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error getting next sort order: {ex.Message}");
			return 1;
		}
	}
}
