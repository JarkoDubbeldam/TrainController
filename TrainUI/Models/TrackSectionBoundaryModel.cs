using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

using Avalonia;

using ReactiveUI;

namespace TrainUI.Models {
  [DataContract]
  public class TrackSectionBoundaryModel : ReactiveObject {
    private Point location;
    [DataMember]
    public Point Location { get => location; set => this.RaiseAndSetIfChanged(ref location, value); }
    [DataMember]
    public int Id { get; internal set; }
  }
}
