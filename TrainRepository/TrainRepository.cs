﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  public sealed class TrainRepository : Repository<Train>, IDisposable {
    private readonly IDisposable subscription;

    public TrainRepository(IZ21Client z21Client) : base(z21Client) {
      subscription = z21Client.LocomotiveInformationChanged.Subscribe(x => Z21Client_LocomotiveInformationChanged(x));
    }

    private void Z21Client_LocomotiveInformationChanged(LocomotiveInformation e) {
      if (repos.TryGetValue(e.Address, out var train) && train.IsCompletedSuccessfully) {
        train.Result.Update(e);
      }
    }


    protected override void ObjectChangedByUserHandler(object sender, PropertyChangedEventArgs e) {
      switch (e.PropertyName) {
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

    protected override async Task<Train> GetObjectInfoFromController(int address, string name) {
      var request = new LocomotiveInformationRequest { LocomotiveAddress = (short)address };
      var response = await client.Value.GetLocomotiveInformation(request);

      var train = new Train(address, name, response.TrainSpeed, response.TrainFunctions);
      return train;
    }

    public override void Dispose() {
      subscription.Dispose();
    }
  }
}
