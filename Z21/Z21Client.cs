using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Z21.API;
using Z21.Domain;

namespace Z21 {
  public partial class Z21Client : IDisposable {
    private readonly IUdpClient udpClient;
    private readonly IObservable<byte[]> inStream;
    private readonly IDisposable keepConnectionAliveSubscription;
    private readonly TimeSpan timeout = TimeSpan.FromSeconds(1);

    public Z21Client(IUdpClient udpClient) {
      this.udpClient = udpClient;
      inStream = udpClient.ObserveBytes();

      var connectionTicker = Observable.Interval(TimeSpan.FromSeconds(2))
        .SelectMany(async x => {
          try {
            await GetSerialNumber(new SerialNumberRequest());
            return true;
          } catch (TimeoutException) {
            return false;
          }
        })
        .Publish();
      keepConnectionAliveSubscription = connectionTicker.Connect();
      ConnectionStatus = connectionTicker.DistinctUntilChanged();
    }

  

    public IObservable<bool> ConnectionStatus { get; } 
    public IObservable<TrackStatus> TrackStatusChanged => GetStream(new TrackStatusResponseFactory());
    public IObservable<SystemState> SystemStateChanged => GetStream(new SystemStateResponseFactory());
    public IObservable<LocomotiveInformation> LocomotiveInformationChanged => GetStream(new LocomotiveInformationResponseFactory());
    public IObservable<TurnoutInformation> TurnoutInformationChanged => GetStream(new TurnoutInformationResponseFactory());
    public IObservable<OccupancyStatus> OccupancyStatusChanged => GetStream(new OccupancyStatusResponseFactory());

    private IObservable<TResponse> GetStream<TResponse>(ResponseFactory<TResponse> factory) => 
      inStream
      .Where(x => MatchesPattern(x, factory.ResponsePattern))
      .Select(factory.ParseResponseBytes);

    private static bool MatchesPattern(byte[] responseBytes, byte?[] pattern) {
      return responseBytes.Zip(pattern, (r, p) => p == null || p == r).All(x => x);
    }

    private async IAsyncEnumerable<bool> KeepConnectionAlive([EnumeratorCancellation] CancellationToken token) {
      while (true) {
        if (token.IsCancellationRequested) yield break;
        const int TWO_SECONDS = 2_000;
        await Task.Delay(TWO_SECONDS);
        bool success;
        try {
          await GetSerialNumber(new SerialNumberRequest());
          success = true;
        } catch (TimeoutException) {
          success = false;
        }
        yield return success;
      }
    }

    private async Task<TResponse> SendRequestWithResponse<TResponse>(RequestWithResponse<TResponse> request) {
      var factory = request.GetResponseFactory();
      var responseTask = CreateResponseTask(factory.ResponsePattern);
      udpClient.SendBytes(request.ToByteArray());
      return factory.ParseResponseBytes(await responseTask);
    }


    private async Task<TOut> SendRequestWithAddressSpecificResponse<TOut>(AddressSpecificRequest<TOut> request) {
      var factory = request.GetResponseFactory();
      var requestBytes = request.ToByteArray();
      var responsePattern = factory.ResponsePattern;
      var responseTask = CreateResponseTask(responsePattern);
      udpClient.SendBytes(requestBytes);
      return factory.ParseResponseBytes(await responseTask);
    }

    private void SendRequestWithoutResponse(Request request) {
      udpClient.SendBytes(request.ToByteArray());
    }

    private IObservable<byte[]> CreateResponseTask(byte?[] pattern) {
      var timeoutSequence = Observable.Throw<byte[]>(new TimeoutException()).DelaySubscription(timeout);
      return Observable.Amb(
        udpClient.ObserveBytes().FirstAsync(x => {
          return MatchesPattern(x, pattern);
        }),
        timeoutSequence);
    }

    private void LogOff() => SendRequestWithoutResponse(new LogOffRequest());

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          keepConnectionAliveSubscription.Dispose();
          LogOff();
          udpClient.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~Z21Client()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
