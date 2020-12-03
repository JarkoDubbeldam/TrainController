using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

using TrainUI.Models;

using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class AddTrainFunctionViewModel : ReactiveObject, IActivatableViewModel {
    private string name;

    public AddTrainFunctionViewModel() {
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable c) => { });
      var okEnabled = this.WhenAnyValue(x => x.Name, name => !string.IsNullOrWhiteSpace(name));
      Ok = ReactiveCommand.Create(
        () => new TrainFunctionRegistration { Name = name, FunctionMask = AccumulateMask() },
        okEnabled);
      Cancel = ReactiveCommand.Create(() => { });

      Toggles = Enum.GetValues(typeof(TrainFunctions))
        .Cast<TrainFunctions>()
        .Where(x => x != TrainFunctions.None)
        .Select(x => new AddTrainFunctionToggleViewModel { Function = x })
        .ToList();
    }

    [DataMember]
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }

    [DataMember]
    public Guid TrainId { get; set; }

    public IReadOnlyCollection<AddTrainFunctionToggleViewModel> Toggles { get; set; }

    public ViewModelActivator Activator { get; }
    public ReactiveCommand<Unit, TrainFunctionRegistration> Ok { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }

    private TrainFunctions AccumulateMask() => Toggles.Aggregate(TrainFunctions.None, (a, f) => f.Active ? f.Function | a : a);
  }
}
