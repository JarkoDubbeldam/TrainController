using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;

using Avalonia.Threading;

using ReactiveUI;

using Splat;

using TrainUI.Models;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainViewModel : ReactiveObject, IActivatableViewModel {
    private string name;
    private int speed;
    private short address;
    private ObservableCollection<TrainFunctionViewModel> trainFunctions;
    private bool enabled;

    public ViewModelActivator Activator { get; }

    public TrainViewModel() {
      Activator = new ViewModelActivator();
      TrainFunctions = new ObservableCollection<TrainFunctionViewModel>();
      var hasNonZeroSpeed = this.WhenAnyValue(x => x.Speed, x => x != 0);
      Stop = ReactiveCommand.Create(() => Speed = 0, hasNonZeroSpeed);
      this.WhenActivated((CompositeDisposable disposables) => {
        var z21Client = Locator.Current.GetService<IZ21Client>();
        z21Client
          .ConnectionStatus
          .Subscribe(x => Enabled = x)
          .DisposeWith(disposables);
        z21Client.SetBroadcastFlags(new SetBroadcastFlagsRequest { BroadcastFlags = z21Client.BroadcastFlags | BroadcastFlags.DrivingAndSwitching });
        z21Client.LocomotiveInformationChanged
          .Where(t => t.Address == address)
          .Subscribe(HandleTrainUpdate)
          .DisposeWith(disposables);

        this.WhenAnyValue(t => t.Speed)
          .DistinctUntilChanged()
          .Select(ParseIntToSpeed)
          .Subscribe(speed => z21Client.SetTrainSpeed(new TrainSpeedRequest { TrainAddress = address, TrainSpeed = speed }))
          .DisposeWith(disposables);

        // subscribe to loco information.
        z21Client.GetLocomotiveInformation(new LocomotiveInformationRequest { LocomotiveAddress = address });
      });
    }


    [DataMember]
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
    [DataMember]
    public short Address { get => address; set => this.RaiseAndSetIfChanged(ref address, value); }

    public int Speed { get => speed; set => this.RaiseAndSetIfChanged(ref speed, value); }

    [DataMember]
    public ObservableCollection<TrainFunctionViewModel> TrainFunctions { get => trainFunctions; set => this.RaiseAndSetIfChanged(ref trainFunctions, value); }
    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Enabled { get => enabled; set => this.RaiseAndSetIfChanged(ref enabled, value); }
    public ReactiveCommand<Unit, int> Stop { get; }
    public IEnumerable<TrainFunctionMenuModel> TrainFunctionMenuItems => TrainFunctions.Select(x => new TrainFunctionMenuModel { Id = x.Id, Name = x.Name });

    public void RemoveTrainFunction(Guid id) {
      var function = TrainFunctions.SingleOrDefault(x => x.Id == id);
      if(function != null) {
        TrainFunctions.Remove(function);
      }
    }

    private void HandleTrainUpdate(LocomotiveInformation obj) {
      int intSpeed = ParseSpeedToInt(obj.TrainSpeed);
      Dispatcher.UIThread.InvokeAsync(() => Speed = intSpeed);
      foreach(var trainFunction in TrainFunctions) {
        Dispatcher.UIThread.InvokeAsync(() => trainFunction.SetTrainFunctionStatus(obj.TrainFunctions));
      }
    }

    private static int ParseSpeedToInt(TrainSpeed speed) {
      var intSpeed = speed.Speed > 0 ? (int)speed.Speed : 0;
      if (speed.DrivingDirection == DrivingDirection.Backward) {
        intSpeed *= -1;
      }
      return intSpeed;
    }

    private static TrainSpeed ParseIntToSpeed(int speed) {
      var direction = speed > 0 ? DrivingDirection.Forward : DrivingDirection.Backward;
      return new TrainSpeed(SpeedStepSetting.Step128, direction, (Speed)Math.Abs(speed));
    }
  }
}
