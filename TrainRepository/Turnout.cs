using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Z21.Domain;

namespace TrainRepository {
  public class Turnout : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;

    public Turnout(int address, string name, TurnoutPosition turnoutPosition) {
      Address = address;
      Name = name;
      TurnoutPosition = turnoutPosition;
    }

    public int Address { get; }
    public string Name { get; }
    public TurnoutPosition TurnoutPosition { get; private set; }

    public override string ToString() {
      return $"Turnout {Name}\n{TurnoutPosition}";
    }

    public void SetPosition(TurnoutPosition position) {
      TurnoutPosition = position;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TurnoutPosition)));
    }



    internal void Update(TurnoutInformation turnoutInformation) {
      if (TurnoutPosition.Equals(turnoutInformation.TurnoutPosition)) { return; }
      TurnoutPosition = turnoutInformation.TurnoutPosition;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("All!"));
    }
  }
}
