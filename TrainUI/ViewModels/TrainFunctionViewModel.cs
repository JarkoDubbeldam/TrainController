using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

using Splat;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainFunctionViewModel : ReactiveObject, IActivatableViewModel {
    private string name;
    private bool? active;
    private TrainFunctions functionMask;

    public TrainFunctionViewModel() {
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable d) => {
        var client = Locator.Current.GetService<IZ21Client>();
        this.WhenAnyValue(x => x.Active)
          .Throttle(TimeSpan.FromMilliseconds(100))
          .DistinctUntilChanged()
          .Subscribe(s => UpdateTrainFunctions(s, client))
          .DisposeWith(d);
      });
    }

    [DataMember]
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
    public bool? Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }

    [DataMember]
    public TrainFunctions FunctionMask { get => functionMask; set => this.RaiseAndSetIfChanged(ref functionMask, value); }
    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();
    [DataMember]
    public short Address { get; set; }
    public ViewModelActivator Activator { get; }

    public void SetTrainFunctionStatus(TrainFunctions trainFunctions) {
      var masked = trainFunctions & FunctionMask;
      if(masked == FunctionMask) {
        Active = true;
      } else if(masked == TrainFunctions.None) {
        Active = false;
      } else {
        Active = null;
      }
    }

    private void UpdateTrainFunctions(bool? s, IZ21Client client) {
      if (s == null) {
        return;
      }

      var relevantFunctions = FunctionMask.GetFlags().Where(x => x != TrainFunctions.None);
      var toggleMode = s.Value ? TrainFunctionToggleMode.On : TrainFunctionToggleMode.Off;
      foreach(var function in relevantFunctions) {
        client.SetTrainFunction(new TrainFunctionRequest { TrainAddress = Address, TrainFunctions = function, TrainFunctionToggleMode = toggleMode });
      }
    }
  }
}
