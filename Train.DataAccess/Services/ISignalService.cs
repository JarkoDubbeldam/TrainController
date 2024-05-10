using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trains.DataAccess.Models;

namespace Trains.DataAccess.Services;
public interface ISignalService {
  Task<List<Signal>> ListSignals(CancellationToken cancellationToken = default);
  Task SaveSignal(Signal signal);
}
