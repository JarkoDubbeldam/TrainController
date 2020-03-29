using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Z21.Domain;

namespace TrainRepository {
  public class Train : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;

    public Train(int address, string name, TrainSpeed speed, TrainFunctions functions) {
      Address = address;
      Name = name;
      Speed = speed;
      Functions = functions;
    }

    public int Address { get; }
    public string Name { get; }

    public TrainSpeed Speed { get; private set; }
    public TrainFunctions Functions { get; private set; }

    public override string ToString() {
      return $"Train {Name}\n{Speed}\n{Functions}";
    }

    public void SetSpeed(TrainSpeed speed) {
      Speed = speed;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Speed)));
    }

    public void SetFunctions(TrainFunctions functions) {
      Functions = functions;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Functions)));
    }

    internal void Update(LocomotiveInformation locomotiveInformation) {
      if(Speed.Equals(locomotiveInformation.TrainSpeed) && Functions == locomotiveInformation.TrainFunctions) { return; }
      Speed = locomotiveInformation.TrainSpeed;
      Functions = locomotiveInformation.TrainFunctions;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("All!"));
    }
  }
}
