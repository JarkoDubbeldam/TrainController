using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21 {
  public partial class Z21Client : IZ21Client {
    public BroadcastFlags BroadcastFlags { get; private set; } = BroadcastFlags.None;

    public IObservable<TurnoutChangingEventArgs> TurnoutChanging => Observable.FromEventPattern<TurnoutChangingEventArgs>(
      handler => TurnoutChangingInternal += handler,
      handler => TurnoutChangingInternal -= handler
    )
      .Select(x => x.EventArgs);


    private event EventHandler<TurnoutChangingEventArgs>? TurnoutChangingInternal;

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

    public void SetTurnout(SetTurnoutRequest request) => SendRequestWithoutResponse(request);

    public Task<OccupancyStatus> GetOccupancyStatus(OccupancyStatusRequest request) => SendRequestWithResponse(request);

    public void SetSignal(SetSignalRequest request) => SendRequestWithoutResponse(request);
  }
}
