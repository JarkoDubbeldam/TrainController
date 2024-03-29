﻿using System;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21 {
  public interface IZ21Client {
    IObservable<bool> ConnectionStatus { get; }
    IObservable<TurnoutChangingEventArgs> TurnoutChanging { get; }
    IObservable<TurnoutInformation> TurnoutInformationChanged { get; }
    IObservable<LocomotiveInformation> LocomotiveInformationChanged { get; }
    IObservable<SystemState> SystemStateChanged { get; }
    IObservable<TrackStatus> TrackStatusChanged { get; }
    IObservable<OccupancyStatus> OccupancyStatusChanged { get; }

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
    Task<TurnoutInformation> SetTurnout(SetTurnoutRequest request);
    Task<OccupancyStatus> GetOccupancyStatus(OccupancyStatusRequest request);
    void SetSignal(SetSignalRequest request);
  }
}