using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;

using Z21.API;
using Z21.Domain;

namespace Z21 {
  public partial class Z21Client : IDisposable {
    private readonly Func<IUdpClient> udpClientFactory;
    private readonly CompositeDisposable disposables = new();
    private readonly Lazy<IObservable<TrackStatus>> lazyTrackStatusChanged;
    private readonly Lazy<IObservable<SystemState>> lazySystemStateChanged;
    private readonly Lazy<IObservable<TurnoutInformation>> lazyTurnoutInformationChanged;
    private readonly Lazy<IObservable<OccupancyStatus>> lazyOccupancyStatusChanged;
    private readonly Lazy<IObservable<LocomotiveInformation>> lazyLocomotiveInformationChanged;

    public Z21Client(Func<IUdpClient> udpClientFactory) {
      this.udpClientFactory = udpClientFactory;

      lazyTrackStatusChanged = new(() => GetStream(new TrackStatusResponseFactory()));
      lazySystemStateChanged = new(() => GetStream(new SystemStateResponseFactory(), BroadcastFlags.Z21SystemState));
      lazyLocomotiveInformationChanged = new(() => GetStream(new LocomotiveInformationResponseFactory(), BroadcastFlags.AllLocs));
      lazyTurnoutInformationChanged = new(() => GetStream(new TurnoutInformationResponseFactory(), BroadcastFlags.DrivingAndSwitching));
      lazyOccupancyStatusChanged = new(() => GetStream(new OccupancyStatusResponseFactory(), BroadcastFlags.RBus));
    }

    public IObservable<TrackStatus> TrackStatusChanged => lazyTrackStatusChanged.Value;
    public IObservable<SystemState> SystemStateChanged => lazySystemStateChanged.Value;
    public IObservable<TurnoutInformation> TurnoutInformationChanged => lazyTurnoutInformationChanged.Value;
    public IObservable<OccupancyStatus> OccupancyStatusChanged => lazyOccupancyStatusChanged.Value;
    public IObservable<LocomotiveInformation> LocomotiveInformationChanged => lazyLocomotiveInformationChanged.Value;

    private IObservable<TResponse> GetStream<TResponse>(ResponseFactory<TResponse> factory, BroadcastFlags requiredFlags = BroadcastFlags.None) {
      var connection = UpdateStreamConnection<TResponse>.CreateUpdateStream(factory, requiredFlags, udpClientFactory);
      disposables.Add(connection);
      return connection.UpdateObservable;
    }

    private async Task<TResponse> SendRequestWithResponse<TResponse>(RequestWithResponse<TResponse> request) {
      var connection = new RequestResponseConnector(udpClientFactory);
      return await connection.Execute(request);
    }


    private async Task<TOut> SendRequestWithAddressSpecificResponse<TOut>(AddressSpecificRequest<TOut> request) {
      var connection = new RequestResponseConnector(udpClientFactory);

      return await connection.Execute(request);
    }

    private void SendRequestWithoutResponse(Request request) {
      using var client = udpClientFactory();
      client.SendBytes(request.ToByteArray());
    }


    public void Dispose() => disposables.Dispose();
  }
}
