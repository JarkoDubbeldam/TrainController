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

    public Task<int> GetSerialNumber(SerialNumberRequest serialNumberRequest) => SendRequestWithResponse<SerialNumberResponse, int>(serialNumberRequest);

    public void SetBroadcastFlags(SetBroadcastFlagsRequest request) {
      BroadcastFlags = request.BroadcastFlags;
      SendRequestWithoutResponse(request);
    }

    public Task<BroadcastFlags> GetBroadcastFlags(BroadcastFlagsRequest request) => SendRequestWithResponse<BroadcastFlagsResponse, BroadcastFlags>(request);
    public Task<TrackStatus> SetTrackStatus(TrackStatusRequest request) => SendRequestWithResponse<TrackStatusResponse, TrackStatus>(request);

    public Task<SystemState> GetSystemState(SystemStateRequest request) => SendRequestWithResponse<SystemStateResponse, SystemState>(request);

    public void SetTrainSpeed(TrainSpeedRequest request) => SendRequestWithoutResponse(request);
    public void SetTrainFunction(TrainFunctionRequest request) => SendRequestWithoutResponse(request);

    public async Task<LocomotiveInformation> GetLocomotiveInformation(LocomotiveInformationRequest request) {
      var factory = new LocomotiveInformationResponse();
      var requestBytes = request.ToByteArray();
      var responsePattern = factory.ResponsePattern.Concat(new byte?[] { requestBytes[5], requestBytes[6] }).ToArray();
      var responseTask = CreateResponseTask(responsePattern);
      udpClient.SendBytes(requestBytes);
      return factory.ParseResponseBytes(await responseTask);
    }
  }
}
