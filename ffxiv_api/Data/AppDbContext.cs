using ffxiv_api.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Duty> Duties => Set<Duty>();

	public DbSet<MentorRouletteLog> MentorRouletteLogs => Set<MentorRouletteLog>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
