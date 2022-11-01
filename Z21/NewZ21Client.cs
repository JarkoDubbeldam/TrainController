using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21;
public class NewZ21Client : IZ21Client, IDisposable {
  private readonly IPEndPoint remoteAddress;
  private readonly CancellationTokenSource cts;

  public IObservable<bool> ConnectionStatus => throw new NotImplementedException();

  public IObservable<TurnoutChangingEventArgs> TurnoutChanging => throw new NotImplementedException();

  public IObservable<TurnoutInformation> TurnoutInformationChanged => OpenUpdateStream(BroadcastFlags.DrivingAndSwitching, new TurnoutInformationResponseFactory())
    .ToObservable();

  public IObservable<LocomotiveInformation> LocomotiveInformationChanged => OpenUpdateStream(BroadcastFlags.AllLocs, new LocomotiveInformationResponseFactory())
    .ToObservable();

  public IObservable<SystemState> SystemStateChanged => OpenUpdateStream(BroadcastFlags.Z21SystemState, new SystemStateResponseFactory())
    .ToObservable();

  public IObservable<TrackStatus> TrackStatusChanged => OpenUpdateStream(BroadcastFlags.None, new TrackStatusResponseFactory())
    .ToObservable();

  public IObservable<OccupancyStatus> OccupancyStatusChanged => OpenUpdateStream(BroadcastFlags.RBus, new OccupancyStatusResponseFactory())
    .ToObservable();

  public BroadcastFlags BroadcastFlags => throw new NotImplementedException();


  public NewZ21Client(IPEndPoint remoteAddress) {
    this.remoteAddress = remoteAddress;
    cts = new CancellationTokenSource();
  }

  private void LogOff(System.Net.Sockets.UdpClient client) {
    var logOff = new LogOffRequest().ToByteArray();
    client.Send(logOff, logOff.Length, remoteAddress);
  }

  private void SendRequestWithoutResponse(Request request) {
    var bytes = request.ToByteArray();
    var client = new System.Net.Sockets.UdpClient();
    try {
      client.Send(bytes, bytes.Length, remoteAddress);
    } finally {
      LogOff(client);
    }
  }

  private async Task<T> SendRequestWithResponse<T>(RequestWithResponse<T> request, CancellationToken cancellationToken = default) {
    var bytes = request.ToByteArray();
    var client = new System.Net.Sockets.UdpClient();
    using var timedCts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken, timedCts.Token);

    try {
      await client.SendAsync(bytes, bytes.Length, remoteAddress);
      UdpReceiveResult response;
      do {
        response = await client.ReceiveAsync(requestCts.Token);
      } while (!response.RemoteEndPoint.Equals(remoteAddress));
      return request.GetResponseFactory().ParseResponseBytes(response.Buffer);
    } finally {
      LogOff(client);
    }
  }

  private IEnumerable<byte[]> SplitMessages(byte[] message) {
    var index = 0;
    while (index < message.Length) {
      var messageLength = message[index];
      var subMessage = message.Skip(index).Take(messageLength).ToArray();
      yield return subMessage;
      index += messageLength;
    }
  }

  private async IAsyncEnumerable<T> OpenUpdateStream<T>(BroadcastFlags broadcastFlags, ResponseFactory<T> responseFactory, [EnumeratorCancellation] CancellationToken ct = default) {
    var client = new System.Net.Sockets.UdpClient();
    var broadcastFlagsRequest = new SetBroadcastFlagsRequest { BroadcastFlags = broadcastFlags }.ToByteArray();
    using var innerCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
    client.Send(broadcastFlagsRequest, broadcastFlagsRequest.Length, remoteAddress);
    try {
      while (!innerCts.IsCancellationRequested) {
        using var timerCts = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(innerCts.Token, timerCts.Token);
        UdpReceiveResult response = default;
        try {
          response = await client.ReceiveAsync(combinedCts.Token);
        } catch (OperationCanceledException) {
          if (!innerCts.IsCancellationRequested) {
            client.Send(broadcastFlagsRequest, broadcastFlagsRequest.Length, remoteAddress);
          }
          continue;
        }
        if (!response.RemoteEndPoint.Equals(remoteAddress)) {
          continue;
        }
        foreach (var submessage in SplitMessages(response.Buffer)) {
          if (responseFactory.MatchesPattern(submessage)) {
            yield return responseFactory.ParseResponseBytes(response.Buffer);
          }
        }
      }
    } finally {
      LogOff(client);
    }
  }

  public void Dispose() {
    cts.Cancel();
    ((IDisposable)cts).Dispose();
  }

  public Task<BroadcastFlags> GetBroadcastFlags(BroadcastFlagsRequest request) => SendRequestWithResponse(request);
  public Task<int> GetSerialNumber(SerialNumberRequest serialNumberRequest) => SendRequestWithResponse(serialNumberRequest);
  public Task<SystemState> GetSystemState(SystemStateRequest request) => SendRequestWithResponse(request);
  public void SetBroadcastFlags(SetBroadcastFlagsRequest request) => SendRequestWithoutResponse(request);
  public Task<TrackStatus> SetTrackStatus(TrackStatusRequest request) => SendRequestWithResponse(request);
  public void SetTrainSpeed(TrainSpeedRequest request) => SendRequestWithoutResponse(request);
  public void SetTrainFunction(TrainFunctionRequest request) => SendRequestWithoutResponse(request);
  public Task<LocomotiveInformation> GetLocomotiveInformation(LocomotiveInformationRequest request) => SendRequestWithResponse(request);
  public Task<TurnoutInformation> GetTurnoutInformation(TurnoutInformationRequest request) => SendRequestWithResponse(request);
  public void SetTurnout(SetTurnoutRequest request) => SendRequestWithoutResponse(request);
  public Task<OccupancyStatus> GetOccupancyStatus(OccupancyStatusRequest request) => SendRequestWithResponse(request);
  public void SetSignal(SetSignalRequest request) => SendRequestWithoutResponse(request);
  public void SendBatchRequests(params Request[] requests) {
    var bytes = requests.SelectMany(x => x.ToByteArray()).ToArray();
    var client = new System.Net.Sockets.UdpClient();
    try {
      client.Send(bytes, bytes.Length, remoteAddress);
    } finally {
      LogOff(client);
    }
  }
}
