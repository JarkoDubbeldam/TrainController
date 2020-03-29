using System.Threading.Tasks;

namespace TrainRepository {
  public interface ITrainRepository {
    Task<Train> GetTrain(int address);
    Task<Train> RegisterTrain(int address, string name);
  }
}