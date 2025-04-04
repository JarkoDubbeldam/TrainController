using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
public interface ISegmentService {
  Task<List<Segment>> ListSegments(CancellationToken cancellationToken = default);
}
