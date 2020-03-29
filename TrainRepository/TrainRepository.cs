using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  public class TrainRepository : ITrainRepository {
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

      var train = new Train(address, name, response.TrainSpeed, response.TrainFunctions);
      train.PropertyChanged += OnRegisteredTrainChanged;
      return train;
    }

    private void OnRegisteredTrainChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      switch(e.PropertyName) {
        case nameof(Train.Speed): {
            var train = (Train)sender;
            var request = new TrainSpeedRequest {
              TrainAddress = (short)train.Address,
              TrainSpeed = train.Speed
            };
            client.Value.SetTrainSpeed(request);
          }
          return;

        case nameof(Train.Functions): {
            var train = (Train)sender;
            var request = new TrainFunctionRequest {
              TrainAddress = (short)train.Address,
              TrainFunctions = train.Functions
            };
            client.Value.SetTrainFunction(request);
          }
          return;

      }

    }
  }
}
