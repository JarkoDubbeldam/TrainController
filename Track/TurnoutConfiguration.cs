using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using ReactiveUI;

using Z21.Domain;

namespace Track {
  public class TurnoutConfiguration : ReactiveObject {
    private bool isActive;

    public int TurnoutId { get; internal set; }
    public TurnoutMode TurnoutMode { get; internal set; }

    public bool IsActive { get => isActive; internal set => this.RaiseAndSetIfChanged(ref isActive, value); }
    internal void CheckIfActive(TurnoutInformation turnoutInformation) {
      Debug.Assert(turnoutInformation.Address == TurnoutId);
      IsActive = (turnoutInformation.TurnoutPosition == TurnoutPosition.Position1 && TurnoutMode == TurnoutMode.Right) ||
        (turnoutInformation.TurnoutPosition == TurnoutPosition.Position2 && TurnoutMode == TurnoutMode.Left);
    }
  }
}
