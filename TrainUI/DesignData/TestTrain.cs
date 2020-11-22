
using System;

using TrainUI.Models;
using TrainUI.ViewModels;

namespace TrainUI.DesignData {
  public class TestTrain : TrainViewModel {
    public TestTrain() : base(null, new TrainModel { Name = "TestTrain", Address = 5, TrainFunctions = Array.Empty<TrainFunctionModel>() }, null) { }
  }
}
