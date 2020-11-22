using System;
using System.Linq;

using Newtonsoft.Json;

using TrainUI.Models;
using TrainUI.ViewModels;

namespace TrainUI.Converters {
  public class TrainModelConverter : ModelViewModelConverter<TrainModel, TrainViewModel> {
    private readonly TrainFunctionModelConverter trainFunctionModelConverter;

    public TrainModelConverter(Func<TrainModel, TrainViewModel> factory, TrainFunctionModelConverter trainFunctionModelConverter) : base(factory) {
      this.trainFunctionModelConverter = trainFunctionModelConverter;
    }

    public override TrainModel ToModel(TrainViewModel viewModel) => new TrainModel { 
      Address = viewModel.Address, 
      Name = viewModel.Name, 
      TrainFunctions = viewModel.TrainFunctionsViewModels.Select(trainFunctionModelConverter.ToModel).ToList()
    };
  }
}
