﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21 {
  public partial class Z21Client : IDisposable {
    private readonly IUdpClient udpClient;
    private readonly UdpMessageHandler udpMessageHandler;
    private readonly CancellationTokenSource cancellationSource;
    private readonly Task keepConnectionAliveTask;

    public Z21Client(IUdpClient udpClient) {
      this.udpClient = udpClient;
      udpMessageHandler = new UdpMessageHandler(udpClient);
      udpMessageHandler.MessageReceived += TrackStatusChangedListener;
      udpMessageHandler.MessageReceived += SystemStateChangedListener;
      udpMessageHandler.MessageReceived += LocomotiveInfoChangedListener;

      cancellationSource = new CancellationTokenSource();
      keepConnectionAliveTask = Task.Run(() => KeepConnectionAlive(cancellationSource.Token), cancellationSource.Token);
    }

    public event EventHandler<TrackStatus> TrackStatusChanged;
    public event EventHandler<SystemState> SystemStateChanged;
    public event EventHandler<LocomotiveInformation> LocomotiveInformationChanged;

    private void TrackStatusChangedListener(object sender, byte[] responseBytes) {
      var trackStatusResponse = new TrackStatusResponse();
      if(responseBytes.Zip(trackStatusResponse.ResponsePattern, (r, p) => p == null || p == r).All(x => x)) {
        TrackStatusChanged?.Invoke(this, trackStatusResponse.ParseResponseBytes(responseBytes));
      }
    }

    private void SystemStateChangedListener(object sender, byte[] responseBytes) {
      var response = new SystemStateResponse();
      if (responseBytes.Zip(response.ResponsePattern, (r, p) => p == null || p == r).All(x => x)) {
        SystemStateChanged?.Invoke(this, response.ParseResponseBytes(responseBytes));
      }
    }

    private void LocomotiveInfoChangedListener(object sender, byte[] responseBytes) {
      var locoResponse = new LocomotiveInformationResponse();
      if(responseBytes.Zip(locoResponse.ResponsePattern, (r, p) => p == null || p == r).All(x => x)) {
        LocomotiveInformationChanged?.Invoke(this, locoResponse.ParseResponseBytes(responseBytes));
      }
    }

    private async Task KeepConnectionAlive(CancellationToken token) {
      while (true) {
        if (token.IsCancellationRequested) return;
        const int THIRTY_SECONDS = 30_000;
        await Task.Delay(THIRTY_SECONDS);
        await GetSerialNumber(new SerialNumberRequest());
      }
    }

    private async Task<TResponse> SendRequestWithResponse<TRequest, TFactory, TResponse>(TRequest request) where TRequest : Request where TFactory : ResponseFactory<TResponse>, new() {
      var factory = new TFactory();
      var responseTask = CreateResponseTask(factory.ResponsePattern);
      udpClient.SendBytes(request.ToByteArray());
      return factory.ParseResponseBytes(await responseTask);
    }

    private void SendRequestWithoutResponse(Request request) {
      udpClient.SendBytes(request.ToByteArray());
    }

    private async Task<byte[]> CreateResponseTask(byte?[] pattern) {
      var promise = new UdpPromise(pattern);
      udpMessageHandler.MessageReceived += promise.RecieveMessageEventHandler;
      try {
        return await promise.Task;
      } finally {
        udpMessageHandler.MessageReceived -= promise.RecieveMessageEventHandler;
      }
    }

    private void LogOff() => SendRequestWithoutResponse(new LogOffRequest());

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          cancellationSource.Cancel();
          
          LogOff();
          udpMessageHandler.Dispose();
          udpClient.Dispose();
          cancellationSource.Dispose();
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
