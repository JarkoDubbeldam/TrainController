using System.Collections.Concurrent;
using System.Reactive.Subjects;

using Microsoft.Extensions.Hosting;

using Z21;
using Z21.Domain;

namespace TrainController.Occupancies;
internal class OccupancyController(IZ21Client z21Client) : BackgroundService, IController<Occupancy> {
  private readonly ConcurrentDictionary<int, Occupancy> occupancies = new();
  private readonly Subject<Occupancy> subject = new();
  public IObservable<Occupancy> Observable => subject;

  public Task Apply(Occupancy value) => throw new NotSupportedException();
  public Task<Occupancy?> Get(int id) => Task.FromResult(occupancies.GetValueOrDefault(id));
  public Task<IReadOnlyDictionary<int, Occupancy>> List() => Task.FromResult<IReadOnlyDictionary<int, Occupancy>>(occupancies);
  protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
    using var _ = z21Client.OccupancyStatusChanged
      .Subscribe(OnStatusChanged);
    await Task.Delay(-1, stoppingToken);
  }

  private void OnStatusChanged(OccupancyStatus status) {
    var offset = (status.GroupIndex) * 80;
    for (var index = 0; index < 80; index++) {
      var occupancy = occupancies.AddOrUpdate(offset + index, id => {
        var createdOccupancy = new Occupancy(Id: id, IsOccupied: status.Occupancies[index]);
        subject.OnNext(createdOccupancy);
        return createdOccupancy;
      },
       (_, o) => {
         var newOccupancy = o with { IsOccupied = status.Occupancies[index] };
         if (newOccupancy != o) {
           subject.OnNext(newOccupancy);
         }
         return newOccupancy;
       });
    }
  }
}
