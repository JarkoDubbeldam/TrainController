using System;
using System.Collections.Generic;
using System.Text;

using TrainUI.Models;

namespace TrainUI.DesignData {
  public class TestMainWindowModel : MainWindowModel {
    public TestMainWindowModel() {
      Trains = new TrainModel[] { new TrainModel { Name = "TestTrain", Address = 5, TrainFunctions = Array.Empty<TrainFunctionModel>() } };
    }
  }
}
