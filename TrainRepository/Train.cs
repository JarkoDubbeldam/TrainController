using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ReactiveUI;
using Z21.Domain;

namespace TrainRepository {
  public class Train : ReactiveObject {
    private TrainSpeed speed;
    private TrainFunctions functions;

    public Train(int address, string name, TrainSpeed speed, TrainFunctions functions) {
      Address = address;
      Name = name;
      Speed = speed;
      Functions = functions;
    }

    public int Address { get; }
    public string Name { get; }

    public TrainSpeed Speed { get => speed; private set => this.RaiseAndSetIfChanged(ref speed, value); }
    public TrainFunctions Functions { get => functions; private set => this.RaiseAndSetIfChanged(ref functions, value); }

    public override string ToString() {
      return $"Train {Name}\n{Speed}\n{Functions}";
    }

    public void SetSpeed(TrainSpeed speed) => Speed = speed;

    public void SetFunctions(TrainFunctions functions) => Functions = functions;

    internal void Update(LocomotiveInformation locomotiveInformation) {
      if (Speed.Equals(locomotiveInformation.TrainSpeed) && Functions == locomotiveInformation.TrainFunctions) { return; }
      using var x = DelayChangeNotifications();
      Speed = locomotiveInformation.TrainSpeed;
      Functions = locomotiveInformation.TrainFunctions;
    }
  }
}
