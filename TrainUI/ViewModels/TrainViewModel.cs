using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
    private TrainFunctions trainFunctions;
    private readonly IZ21Client z21Client;
    private readonly TrainModel trainModel;
    private readonly Func<TrainFunctionModel, TrainFunctionViewModel> trainFunctionFactory;

    public ViewModelActivator Activator { get; }
    public TrainViewModel(IZ21Client z21Client, TrainModel trainModel, Func<TrainFunctionModel, TrainFunctionViewModel> trainFunctionFactory) {
      this.z21Client = z21Client;
      this.trainModel = trainModel;
      this.trainFunctionFactory = trainFunctionFactory;
      //var _ = Connect();
      Activator = new ViewModelActivator();


      var speedNonZero = this.WhenAnyValue(x => x.Speed, x => x != 0);
      Stop = ReactiveCommand.Create(() => { Speed = 0; }, speedNonZero);

      TrainFunctionsViewModels = new ObservableCollection<TrainFunctionViewModel>(trainModel.TrainFunctions.Select(x => trainFunctionFactory(x)));


      this.WhenActivated(() =>
        new[] {
          z21Client.LocomotiveInformationChanged.Where(x => x.Address == Address).Subscribe(HandleTrainUpdate),
          this.WhenAnyValue(x => x.Speed).Throttle(TimeSpan.FromMilliseconds(50)).DistinctUntilChanged().Subscribe(UpdateSpeed),
          TrainFunctionsViewModels
            .Select(x => x.WhenAnyValue(y => y.Active, y => y.Mask)
              .DistinctUntilChanged()
              .Select(GetNewTrainFunctions))
            .Merge()
            .Throttle(TimeSpan.FromMilliseconds(50))
            .DistinctUntilChanged()
            .Subscribe(UpdateFunctions),
          TrainFunctionsViewModels
            .Select(x => x.WhenAnyValue(y => y.Active, y => y.Mask)
              .DistinctUntilChanged()
              .Select(GetNewTrainFunctions))
            .Merge()
            .Subscribe(x => Debug.WriteLine(x))
        });
    }

    public async Task Connect() {
      z21Client.SetBroadcastFlags(new SetBroadcastFlagsRequest { BroadcastFlags = z21Client.BroadcastFlags | BroadcastFlags.DrivingAndSwitching });
      await z21Client.GetLocomotiveInformation(new LocomotiveInformationRequest { LocomotiveAddress = Address });
    }

    [DataMember]
    public string Name => trainModel.Name;
    [DataMember]
    public short Address => trainModel.Address;
    [DataMember]
    public ObservableCollection<TrainFunctionViewModel> TrainFunctionsViewModels { get; }

    [IgnoreDataMember]
    public int Speed { get => speed; set => this.RaiseAndSetIfChanged(ref speed, value); }
    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> Stop { get; }

    private void HandleTrainUpdate(LocomotiveInformation locomotiveInformation) {
      var newSpeed = ParseSpeed(locomotiveInformation.TrainSpeed);
      if(newSpeed != Speed) {
        Dispatcher.UIThread.InvokeAsync(() => Speed = newSpeed);
      }
      var newFunctions = locomotiveInformation.TrainFunctions;
      if (newFunctions != trainFunctions) {
        trainFunctions = newFunctions;
        Dispatcher.UIThread.InvokeAsync(() => {
          foreach (var trainFunctionsViewModel in TrainFunctionsViewModels) {
            trainFunctionsViewModel.TrainFunctions = trainFunctions;
          }
        });
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

    private TrainFunctions GetNewTrainFunctions((bool? active, TrainFunctions mask) data) {
      var (active, mask) = data;
      if(active == null) {
        return trainFunctions;
      }
      return (active ?? false) ? mask | trainFunctions : ~mask & trainFunctions;
    }

    private void UpdateFunctions(TrainFunctions newFunctions) {
      if(newFunctions == trainFunctions) { return; }
      z21Client.SetTrainFunction(new TrainFunctionRequest {
        TrainAddress = Address,
        TrainFunctions = newFunctions
      });
    }
  }
}
