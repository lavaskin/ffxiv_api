using ffxiv_api.Data;
using ffxiv_api.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

public class DutyService
{
	public async Task<DutyModel?> GetDutyAsync(AppDbContext context, long dutyId)
	{
		return await context.Duties.FindAsync(dutyId);
	}

	public async Task<bool> CheckIfDutyNameExistsAsync(AppDbContext context, string dutyName)
	{
		try
		{
			string normalizedName = dutyName.Trim().ToLower();
			return await context.Duties.AnyAsync(duty => duty.Name.ToLower() == normalizedName);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error checking if duty name exists: {ex.Message}");
			return false;
		}
	}
	
	public async Task<bool> CheckIfDutyHasLogsAsync(AppDbContext context, long dutyId)
	{
		try
		{
			var hasLogs = await context.MentorRouletteLogs.AnyAsync(log => log.DutyId == dutyId);
			return hasLogs;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error checking if duty has logs: {ex.Message}");
			return false;
		}
	}
}
