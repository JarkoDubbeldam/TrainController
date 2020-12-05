using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21 {
  public partial class Z21Client : IZ21Client {
    public BroadcastFlags BroadcastFlags { get; private set; } = BroadcastFlags.None;


    public Task<int> GetSerialNumber(SerialNumberRequest serialNumberRequest) => SendRequestWithResponse(serialNumberRequest);

    public void SetBroadcastFlags(SetBroadcastFlagsRequest request) {
      SendRequestWithoutResponse(request);
      _ = Task.Run(async () => BroadcastFlags = await GetBroadcastFlags(new BroadcastFlagsRequest { }));
    }

    public Task<BroadcastFlags> GetBroadcastFlags(BroadcastFlagsRequest request) => SendRequestWithResponse(request);
    public Task<TrackStatus> SetTrackStatus(TrackStatusRequest request) => SendRequestWithResponse(request);

    public Task<SystemState> GetSystemState(SystemStateRequest request) => SendRequestWithResponse<SystemState>(request);

    public void SetTrainSpeed(TrainSpeedRequest request) => SendRequestWithoutResponse(request);
    public void SetTrainFunction(TrainFunctionRequest request) => SendRequestWithoutResponse(request);

    public Task<LocomotiveInformation> GetLocomotiveInformation(LocomotiveInformationRequest request) => SendRequestWithAddressSpecificResponse(request);

    public Task<TurnoutInformation> GetTurnoutInformation(TurnoutInformationRequest request) => SendRequestWithAddressSpecificResponse(request);

    public Task<TurnoutInformation> SetTurnout(SetTurnoutRequest request) => SendRequestWithAddressSpecificResponse(request);

    public Task<OccupancyStatus> GetOccupancyStatus(OccupancyStatusRequest request) => SendRequestWithResponse(request);
  }
}
