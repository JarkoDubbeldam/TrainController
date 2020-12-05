using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class AddTrainFunctionToggleViewModel : ReactiveObject {
    private TrainFunctions function;
    private bool active;

    [DataMember]
    public TrainFunctions Function { get => function; set => this.RaiseAndSetIfChanged(ref function, value); }
    [DataMember]
    public bool Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }
  }
}
