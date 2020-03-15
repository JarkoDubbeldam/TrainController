using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  internal class TrainRepository : ITrainRepository {
    private readonly Lazy<IZ21Client> client;
    private readonly ConcurrentDictionary<int, Task<Train>> repos;

    public TrainRepository(IZ21Client z21Client) {
      this.client = new Lazy<IZ21Client>(() => {
        z21Client.SetBroadcastFlags(new SetBroadcastFlagsRequest { BroadcastFlags = z21Client.BroadcastFlags | BroadcastFlags.DrivingAndSwitching });
        return z21Client;
      }, false);
      repos = new ConcurrentDictionary<int, Task<Train>>();
      z21Client.LocomotiveInformationChanged += Z21Client_LocomotiveInformationChanged;
    }

    private void Z21Client_LocomotiveInformationChanged(object sender, LocomotiveInformation e) {
      repos.TryGetValue(e.Address, out var train);
      if (train.IsCompletedSuccessfully) {
        train.Result.Update(e);
      }
    }

    public Task<Train> GetTrain(int address) {
      if(!repos.TryGetValue(address, out var train)) {
        throw new KeyNotFoundException();
      }
      return train;
    }

    public Task<Train> RegisterTrain(int address, string name) {
      return repos.GetOrAdd(address, GetTrainInfo, name);
    }

    private async Task<Train> GetTrainInfo(int address, string name) {
      var request = new LocomotiveInformationRequest { LocomotiveAddress = (short)address };
      var response = await client.Value.GetLocomotiveInformation(request);

      return new Train { Name = name, Functions = response.TrainFunctions, Speed = response.TrainSpeed };
    }
  }
}
