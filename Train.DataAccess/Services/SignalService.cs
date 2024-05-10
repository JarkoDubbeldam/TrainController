using Microsoft.EntityFrameworkCore;
using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
internal class SignalService(TrainContext trainContext) : ISignalService {
  public Task<List<Signal>> ListSignals(CancellationToken cancellationToken = default) => trainContext.Signals.ToListAsync(cancellationToken);
  public async Task SaveSignal(Signal signal) {
    using var transaction = await trainContext.Database.BeginTransactionAsync();
    try {
      var databaseSignal = await trainContext.Signals.SingleOrDefaultAsync(t => t.Id == signal.Id);
      if (databaseSignal == null) {
        await trainContext.Signals.AddAsync(signal);
      }
      await trainContext.SaveChangesAsync();
      await transaction.CommitAsync();
    } catch (DbUpdateException) {
      await transaction.RollbackAsync();
    }
  }
}
