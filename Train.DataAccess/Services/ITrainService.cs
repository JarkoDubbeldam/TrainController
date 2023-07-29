using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
public interface ITrainService {
  Task<List<Train>> ListTrains(CancellationToken cancellationToken = default);
  Task SaveTrain(Train train);
}
