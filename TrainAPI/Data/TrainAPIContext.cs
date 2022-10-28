using Microsoft.EntityFrameworkCore;
using TrainAPI.Models;

namespace TrainAPI.Data {
  public class TrainAPIContext : DbContext {
    public TrainAPIContext(DbContextOptions<TrainAPIContext> options)
        : base(options) {
    }

    public DbSet<TrainAPI.Models.Train> Train { get; set; } = default!;
    public DbSet<TrainAPI.Models.Turnout> Turnout { get; set; } = default!;
    public DbSet<TrainAPI.Models.TrackSection> TrackSection { get; set; } = default!;
    public DbSet<KeyValue> KeyValue { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<TrackSection>(ts => {
        ts.HasOne(e => e.FromBoundary).WithMany(e => e.FromTrackSections).OnDelete(DeleteBehavior.NoAction);
        ts.HasOne(e => e.ToBoundary).WithMany(e => e.ToTrackSections).OnDelete(DeleteBehavior.NoAction);
      });

      modelBuilder.Entity<KeyValue>(kv => {
        kv.HasKey(x => x.Id);
      });
    }
  }
}
