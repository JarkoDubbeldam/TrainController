using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21 {
  public partial class Z21Client {

    public Task<int> GetSerialNumber(SerialNumberRequest serialNumberRequest) => SendRequestWithResponse<SerialNumberRequest, SerialNumberResponse, int>(serialNumberRequest);

    public void SetBroadcastFlags(SetBroadcastFlagsRequest request) => SendRequestWithoutResponse(request);
    public Task<BroadcastFlags> GetBroadcastFlags(BroadcastFlagsRequest request) => SendRequestWithResponse<BroadcastFlagsRequest, BroadcastFlagsResponse, BroadcastFlags>(request);
    public Task<TrackStatus> SetTrackStatus(TrackStatusRequest request) => SendRequestWithResponse<TrackStatusRequest, TrackStatusResponse, TrackStatus>(request);

    public Task<SystemState> GetSystemState(SystemStateRequest request) => SendRequestWithResponse<SystemStateRequest, SystemStateResponse, SystemState>(request);

    public void SetTrainSpeed(TrainSpeedRequest request) => SendRequestWithoutResponse(request);
  }
}
