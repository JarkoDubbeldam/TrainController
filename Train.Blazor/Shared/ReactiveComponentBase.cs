using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using System.Threading.Tasks;
using System;

namespace Trains.Blazor.Shared;

public class ReactiveComponentBase : ComponentBase, IDisposable {

  private readonly Subject<Unit> _parametersSet = new();
  private readonly Subject<Unit> _disposed = new();

  public IObservable<Unit> Disposed => _disposed.AsObservable();
  public IObservable<Unit> ParametersSet => _parametersSet.AsObservable().TakeUntil(Disposed);

  /// <summary>
  /// Turns a parameter property into IObservable using <see cref="ParametersSet"/> observable
  /// 
  /// It only emmits, when value is changed (DistinctUntilChanged)
  /// The observable completes on <see cref="Dispose"/> (TakeUntil(<see cref="Disposed")/>
  /// </summary>
  /// <param name="parameterSelector">Parameter Property to observe</param>
  /// <example>
  /// <![CDATA[
  /// this.ObserveParameter(() => Id)
  ///     .Select((id, ct) => LoadAsync(id, ct)
  ///     .Switch()
  ///     .Subscribe()
  /// ]]>
  /// </example>
  public IObservable<T> ObserveParameter<T>(Func<T> parameterSelector) {
    return ParametersSet.Select(_ => parameterSelector())
        .DistinctUntilChanged()
        .TakeUntil(_disposed);
  }

  public async override Task SetParametersAsync(ParameterView parameters) {
    await base.SetParametersAsync(parameters);
    _parametersSet.OnNext(Unit.Default);
  }


  public virtual void Dispose() {
    GC.SuppressFinalize(this);
    _disposed.OnNext(Unit.Default);
  }
}
