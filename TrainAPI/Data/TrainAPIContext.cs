using Microsoft.EntityFrameworkCore;

namespace TrainAPI.Data {
  public class TrainAPIContext : DbContext {
    public TrainAPIContext(DbContextOptions<TrainAPIContext> options)
        : base(options) {
    }

    public DbSet<TrainAPI.Models.Train> Train { get; set; } = default!;
  }
}
