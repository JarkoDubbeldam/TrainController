using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
public interface ITurnoutService {
  Task<List<Turnout>> ListTurnouts(CancellationToken cancellationToken = default);
  Task SaveTurnout(Turnout turnout);
}
