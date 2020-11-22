using System;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Threading;

using ReactiveUI;

using TrainUI.Converters;
using TrainUI.Models;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainFunctionViewModel : ReactiveObject, IActivatableViewModel {
    private bool? active;
    private readonly TrainFunctionModel model;

    public TrainFunctionViewModel(TrainFunctionModel model) {
      this.model = model;

      Activator = new ViewModelActivator();
      this.WhenActivated(() =>
        new IDisposable[] { });
    }

    [IgnoreDataMember]
    public ViewModelActivator Activator { get; }

    [DataMember]
    public string Name => model.Name;
    [DataMember]
    public TrainFunctions Mask => model.Mask;

    [IgnoreDataMember]
    public bool? Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }
    public TrainFunctions TrainFunctions { 
      set {
        if ((value & Mask) == Mask) {
          Active = true;
        } else if((value & Mask) == TrainFunctions.None) {
          Active = false;
        } else {
          Active = null;
        }
      } 
    }

  }
}
