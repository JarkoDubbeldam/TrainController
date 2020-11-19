using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TrainUI.Models;
using TrainUI.ViewModels;

namespace TrainUI.Converters {
  public class MainWindowModelConverter : ModelViewModelConverter<MainWindowModel, MainWindowViewModel> {
    private readonly TrainModelConverter trainModelConverter;

    public MainWindowModelConverter(Func<MainWindowModel, MainWindowViewModel> factory, TrainModelConverter trainModelConverter) : base(factory) {
      this.trainModelConverter = trainModelConverter;
    }

    public override MainWindowModel ToModel(MainWindowViewModel viewModel) => new MainWindowModel { Trains = viewModel.Trains.Select(trainModelConverter.ToModel).ToList() };
  }
}
