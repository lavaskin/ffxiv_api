using Microsoft.EntityFrameworkCore;
using ffxiv_api.Models.Entity;

namespace ffxiv_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
		DbContextOptions<AppDbContext> options
	) : base(options)
    {
    }

    public DbSet<DutyModel> Duties { get; set; }
    public DbSet<MentorRouletteLogModel> MentorRouletteLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DutyModel
        modelBuilder.Entity<DutyModel>(entity =>
        {
            entity.ToTable("duty");
            entity.HasKey(e => e.DutyId);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.DutyTypeId).IsRequired(false);
            entity.Property(e => e.ExpansionId).IsRequired(false);
            entity.Property(e => e.LevelRequirement).IsRequired();
            
        });

        // Configure MentorRouletteLog
        modelBuilder.Entity<MentorRouletteLogModel>(entity =>
        {
            entity.ToTable("mentor_roulette_log");
            entity.HasKey(e => e.MentorRouletteLogId);
            entity.Property(e => e.PlayedJobId).IsRequired();
            entity.Property(e => e.Notes).IsRequired();
            entity.Property(e => e.DatePlayed).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.Completed).IsRequired();

            // Configure foreign key relationship
            entity.HasOne(e => e.DutyModel)
                .WithMany()
                .HasForeignKey(e => e.DutyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
