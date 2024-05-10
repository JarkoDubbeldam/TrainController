using Microsoft.EntityFrameworkCore;
using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
internal class TurnoutService(TrainContext trainContext) : ITurnoutService {
  public Task<List<Turnout>> ListTurnouts(CancellationToken cancellationToken = default) => trainContext.Turnouts.ToListAsync(cancellationToken);
  public async Task SaveTurnout(Turnout turnout) {
    using var transaction = await trainContext.Database.BeginTransactionAsync();
    try {
      var dbTurnout = await trainContext.Turnouts.SingleOrDefaultAsync(t => t.Id == turnout.Id);
      if (dbTurnout == null) {
        await trainContext.Turnouts.AddAsync(turnout);
      }
      await trainContext.SaveChangesAsync();
      await transaction.CommitAsync();
    } catch (DbUpdateException) {
      await transaction.RollbackAsync();
    }
  }
}
