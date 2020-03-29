using System;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21 {
  public interface IZ21Client {
    event EventHandler<LocomotiveInformation> LocomotiveInformationChanged;
    event EventHandler<SystemState> SystemStateChanged;
    event EventHandler<TrackStatus> TrackStatusChanged;

    Task<BroadcastFlags> GetBroadcastFlags(BroadcastFlagsRequest request);
    Task<int> GetSerialNumber(SerialNumberRequest serialNumberRequest);
    Task<SystemState> GetSystemState(SystemStateRequest request);
    void SetBroadcastFlags(SetBroadcastFlagsRequest request);
    BroadcastFlags BroadcastFlags { get; }
    Task<TrackStatus> SetTrackStatus(TrackStatusRequest request);
    void SetTrainSpeed(TrainSpeedRequest request);
    void SetTrainFunction(TrainFunctionRequest request);
    Task<LocomotiveInformation> GetLocomotiveInformation(LocomotiveInformationRequest request);
    Task<TurnoutInformation> GetTurnoutInformation(TurnoutInformationRequest request);
  }
}