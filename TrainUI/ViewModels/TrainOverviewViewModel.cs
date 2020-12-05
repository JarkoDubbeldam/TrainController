using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;

using Avalonia;

using ReactiveUI;

using TrainUI.Models;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainOverviewViewModel : ReactiveObject, IActivatableViewModel {
    private ReactiveObject content;
    private TrainListViewModel trainList;

    public TrainOverviewViewModel() {
      Activator = new ViewModelActivator();
      Content = TrainList = new TrainListViewModel();
      this.WhenActivated((CompositeDisposable disposables) => {
        var disposable = Content switch {
          AddTrainViewModel addTrain => SubscribeToAddTrainViewModelResult(addTrain),
          AddTrainFunctionViewModel addTrainFunction => SubscribeToAddTrainFunctionViewModelResult(addTrainFunction),
          _ => null
        };
        disposable?.DisposeWith(disposables);
      });
    }

    [DataMember]
    public ReactiveObject Content { get => content; set => this.RaiseAndSetIfChanged(ref content, value); }

    public ViewModelActivator Activator { get; }
    [DataMember]
    public TrainListViewModel TrainList { get => trainList; internal set => this.RaiseAndSetIfChanged(ref trainList, value); }

    public void AddTrain() {
      var vm = new AddTrainViewModel();
      SubscribeToAddTrainViewModelResult(vm);
      Content = vm;
    }

    public void RemoveTrain(Guid id) {
      var train = TrainList.Trains.SingleOrDefault(train => train.Id == id);
      if(train != null) {
        TrainList.Trains.Remove(train);
      }
    }

    public void AddTrainFunction(Guid trainId) {
      var train = TrainList.Trains.Single(train => trainId == train.Id);
      var vm = new AddTrainFunctionViewModel { TrainId = train.Id };
      SubscribeToAddTrainFunctionViewModelResult(vm);
      Content = vm;
    }

    private IDisposable SubscribeToAddTrainViewModelResult(AddTrainViewModel viewModel) {
      return Observable.Merge(
        viewModel.Ok,
        viewModel.Cancel.Select(x => (TrainRegistration)null))
        .Take(1)
        .Subscribe(model => {
          if (model != null) {
            TrainList.Trains.Add(new TrainViewModel { Name = model.Name, Address = model.Address });
          }
          Content = TrainList;
        });
    }

    private IDisposable SubscribeToAddTrainFunctionViewModelResult(AddTrainFunctionViewModel viewModel) {
      return Observable.Merge(
        viewModel.Ok,
        viewModel.Cancel.Select(x => (TrainFunctionRegistration)null))
        .Take(1)
        .Subscribe(model => {
          Content = TrainList;
          if (model != null) {
            var train = TrainList
              .Trains
              .Single(x => x.Id == viewModel.TrainId);
            try {
              train
                .TrainFunctions
                .Add(new TrainFunctionViewModel { Name = model.Name, FunctionMask = model.FunctionMask, Address = train.Address });
            } catch (AvaloniaInternalException) {
              // ¯\_(ツ)_/¯
            }
          }
        });
    }
  }
}
