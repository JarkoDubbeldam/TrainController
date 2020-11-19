using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using Avalonia.Threading;

using ReactiveUI;

using TrainUI.Models;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainViewModel : ReactiveObject, IActivatableViewModel {
    private int speed;
    private readonly IZ21Client z21Client;
    private readonly TrainModel trainModel;

    public ViewModelActivator Activator { get; }
    public TrainViewModel(IZ21Client z21Client, TrainModel trainModel) {
      this.z21Client = z21Client;
      this.trainModel = trainModel;
      z21Client.SetBroadcastFlags(new SetBroadcastFlagsRequest { BroadcastFlags = z21Client.BroadcastFlags | BroadcastFlags.DrivingAndSwitching });
      z21Client.GetLocomotiveInformation(new LocomotiveInformationRequest { LocomotiveAddress = Address });
      Activator = new ViewModelActivator();
      this.WhenActivated(() => 
        new[] { 
          z21Client.LocomotiveInformationChanged.Where(x => x.Address == Address).Subscribe(HandleTrainUpdate),
          this.WhenAnyValue(x => x.Speed).Throttle(TimeSpan.FromMilliseconds(50)).DistinctUntilChanged().Subscribe(UpdateSpeed)
        });
       

      var speedNonZero = this.WhenAnyValue(x => x.Speed, x => x != 0);
      Stop = ReactiveCommand.Create(() => { Speed = 0; }, speedNonZero);


        
    }

    [DataMember]
    public string Name => trainModel.Name;
    [DataMember]
    public short Address => trainModel.Address;
    [IgnoreDataMember]
    public int Speed { get => speed; set => this.RaiseAndSetIfChanged(ref speed, value); }
    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> Stop { get; }

    private void HandleTrainUpdate(LocomotiveInformation locomotiveInformation) {
      var newSpeed = ParseSpeed(locomotiveInformation.TrainSpeed);
      if(newSpeed != Speed) {
        Dispatcher.UIThread.InvokeAsync(() => Speed = newSpeed);;
      }
    }

    private int ParseSpeed(TrainSpeed speed) {
      var sign = speed.drivingDirection == DrivingDirection.Forward ? 1 : -1;
      return sign * (speed.speed > 0 ? (int)speed.speed : 0);
    }

    private void UpdateSpeed(int newSpeed) {
      z21Client.SetTrainSpeed(new TrainSpeedRequest {
        TrainAddress = Address,
        TrainSpeed = new TrainSpeed(SpeedStepSetting.Step128, newSpeed > 0 ? DrivingDirection.Forward : DrivingDirection.Backward, (Speed)Math.Abs(newSpeed))
      });
    }
  }
}
