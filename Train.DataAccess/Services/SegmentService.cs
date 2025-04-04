using Microsoft.EntityFrameworkCore;

using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
internal class SegmentService(TrainContext trainContext) : ISegmentService {
  public Task<List<Segment>> ListSegments(CancellationToken cancellationToken = default) => trainContext.Segments.ToListAsync(cancellationToken);
}
