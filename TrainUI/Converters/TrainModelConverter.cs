using System;

using Newtonsoft.Json;

using TrainUI.Models;
using TrainUI.ViewModels;

namespace TrainUI.Converters {
  public class TrainModelConverter : ModelViewModelConverter<TrainModel, TrainViewModel> {
    public TrainModelConverter(Func<TrainModel, TrainViewModel> factory) : base(factory) {
    }

    public override TrainModel ToModel(TrainViewModel viewModel) => new TrainModel { Address = viewModel.Address, Name = viewModel.Name };
  }
}
