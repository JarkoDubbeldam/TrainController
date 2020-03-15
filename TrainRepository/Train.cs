using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Z21.Domain;

namespace TrainRepository {
  public class Train : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name { get; internal set; }

    public TrainSpeed Speed { get; internal set; }
    public TrainFunctions Functions { get; internal set; }

    public override string ToString() {
      return $"Train {Name}\n{Speed}\n{Functions}";
    }

    internal void Update(LocomotiveInformation locomotiveInformation) {
      Speed = locomotiveInformation.TrainSpeed;
      Functions = locomotiveInformation.TrainFunctions;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("All!"));
    }
  }
}
