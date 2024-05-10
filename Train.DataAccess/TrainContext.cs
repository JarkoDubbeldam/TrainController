using Microsoft.EntityFrameworkCore;
using Trains.DataAccess.Models;

namespace Trains.DataAccess {
  public class TrainContext : DbContext {
    public TrainContext(DbContextOptions options) : base(options) {
    }

    public DbSet<Train> Trains { get; set; }
    public DbSet<Turnout> Turnouts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
      modelBuilder.Entity<Train>(b => {
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedNever();
        b.Property(t => t.Name).IsRequired();
      })
      .Entity<Turnout>(b => {
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedNever();
      });

  }
}
