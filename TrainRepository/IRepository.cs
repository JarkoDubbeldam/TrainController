using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TrainRepository {
  public interface IRepository<T> : IDisposable {
    Task<T> GetObject(int address);
    Task<T> RegisterObject(int address, string name);
  }
}
