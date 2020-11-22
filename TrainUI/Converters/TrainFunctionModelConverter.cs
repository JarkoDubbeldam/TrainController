using System;
using System.Collections.Generic;
using System.Text;

using TrainUI.Models;
using TrainUI.ViewModels;

namespace TrainUI.Converters {
  public class TrainFunctionModelConverter : ModelViewModelConverter<TrainFunctionModel, TrainFunctionViewModel> {
    public TrainFunctionModelConverter(Func<TrainFunctionModel, TrainFunctionViewModel> factory) : base(factory) {
    }

    public override TrainFunctionModel ToModel(TrainFunctionViewModel viewModel) => new TrainFunctionModel { Mask = viewModel.Mask, Name = viewModel.Name };
  }
}
