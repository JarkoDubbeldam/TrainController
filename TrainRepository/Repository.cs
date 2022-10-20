using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  public abstract class Repository<TObject> : IRepository<TObject> where TObject : INotifyPropertyChanged {
    private protected readonly ConcurrentDictionary<int, Task<TObject>> repos = new ConcurrentDictionary<int, Task<TObject>>();
    private protected readonly Lazy<IZ21Client> client;

    protected Repository(IZ21Client z21Client) {
      this.client = new Lazy<IZ21Client>(() => {
        z21Client.SetBroadcastFlags(new SetBroadcastFlagsRequest { BroadcastFlags = z21Client.BroadcastFlags | BroadcastFlags.DrivingAndSwitching });
        return z21Client;
      }, false);
    }

    public Task<TObject> GetObject(int address) {
      if (!repos.TryGetValue(address, out var item)) {
        throw new KeyNotFoundException();
      }
      return item;
    }

    public Task<TObject> RegisterObject(int address, string name) {
      return repos.GetOrAdd(address, GetObjectInfo, name);
    }

    private async Task<TObject> GetObjectInfo(int address, string name) {
      var tObject = await GetObjectInfoFromController(address, name);
      tObject.PropertyChanged += ObjectChangedByUserHandler;
      return tObject;
    }
    public async Task<IReadOnlyList<TObject>> GetAllObjects() => (await Task.WhenAll(repos.Values)).ToList();
    public void Remove(int address) => repos.Remove(address, out var _);

    protected abstract void ObjectChangedByUserHandler(object sender, PropertyChangedEventArgs e);
    protected abstract Task<TObject> GetObjectInfoFromController(int address, string name);
    public abstract void Dispose();
  }
}
