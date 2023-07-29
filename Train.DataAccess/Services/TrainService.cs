using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
internal class TrainService : ITrainService {
  private readonly TrainContext trainContext;

  public TrainService(TrainContext trainContext) {
    this.trainContext = trainContext;
  }

  public Task<List<Train>> ListTrains(CancellationToken cancellationToken = default) {
    return trainContext.Trains.ToListAsync(cancellationToken);
  }

  public async Task SaveTrain(Train train) {
    using var transaction = await trainContext.Database.BeginTransactionAsync();
    try {
      var databaseTrain = await trainContext.Trains.SingleOrDefaultAsync(t => t.Id == train.Id);
      if (databaseTrain == null) {
        await trainContext.Trains.AddAsync(train);
      } else { 
        databaseTrain.Name = train.Name;
        databaseTrain.Icon = train.Icon;
      }
      await trainContext.SaveChangesAsync();
      await transaction.CommitAsync();
    } catch (DbUpdateException) {
      await transaction.RollbackAsync();
    }

  }
}
