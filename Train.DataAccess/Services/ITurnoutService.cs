using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
public interface ITurnoutService {
  Task<List<Turnout>> ListTurnout(CancellationToken cancellationToken = default);
  Task SaveTurnout(Turnout turnout);
}
