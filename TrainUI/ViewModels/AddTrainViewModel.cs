using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

using TrainUI.Models;

namespace TrainUI.ViewModels {
  [DataContract]
  public class AddTrainViewModel : ReactiveObject, IActivatableViewModel {
    private string name;
    private short address;

    public AddTrainViewModel() {
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable c) => {
      });

      var okEnabled = this.WhenAnyValue(x => x.Name, x => x.Address, (name, address) => !string.IsNullOrWhiteSpace(name) && address > 0);
      Ok = ReactiveCommand.Create(
        () => new TrainRegistration { Name = name, Address = address },
        okEnabled);
      Cancel = ReactiveCommand.Create(() => { });
    }

    public ViewModelActivator Activator { get; }

    [DataMember]
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }

    [DataMember]
    public short Address { get => address; set => this.RaiseAndSetIfChanged(ref address, value); }

    public ReactiveCommand<Unit, TrainRegistration> Ok { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }
  }
}
