namespace TrainController; 
public interface IController<T> where T : class {
  Task Apply(T value);
  Task<T?> Get(int id);
  Task<IReadOnlyDictionary<int, T>> List();
  IObservable<T> Observable { get; }
}
