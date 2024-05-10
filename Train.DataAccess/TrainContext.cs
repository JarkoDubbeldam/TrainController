using Microsoft.EntityFrameworkCore;
using Trains.DataAccess.Models;

namespace Trains.DataAccess {
  public class TrainContext(DbContextOptions options) : DbContext(options) {
    public DbSet<Train> Trains { get; set; }
    public DbSet<Turnout> Turnouts { get; set; }
    public DbSet<Signal> Signals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
      modelBuilder.Entity<Train>(b => {
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedNever();
        b.Property(t => t.Name).IsRequired();
      })
      .Entity<Turnout>(b => {
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedNever();
      })
      .Entity<Signal>(b => {
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedNever();
      });
  }
}
