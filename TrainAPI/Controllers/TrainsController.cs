using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainAPI.Data;
using TrainAPI.Models;
using TrainAPI.Trackers;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainAPI.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class TrainsController : ControllerBase {
    private readonly ITracker<TrainWithInformation> trainTracker;
    private readonly IZ21Client z21Client;

    public TrainsController(ITracker<TrainWithInformation> trainTracker, IZ21Client z21Client) {
      this.trainTracker = trainTracker;
      this.z21Client = z21Client;
    }

    // GET: api/Trains
    [HttpGet]
    public IEnumerable<TrainWithInformation> GetTrain() {
      return trainTracker.List();
    }

    [HttpPost("{id}/Functions")]
    public void SetFunctions(int id, [FromBody] TrainFunctions trainFunctions) {
      var requests = Enum.GetValues<TrainFunctions>()
        .Except(new[] { TrainFunctions.DoubleTraction, TrainFunctions.None, TrainFunctions.Smartsearch })
        .Select(x => new TrainFunctionRequest {
          TrainAddress = (short)id,
          TrainFunctions = x,
          TrainFunctionToggleMode = trainFunctions.HasFlag(x) ? TrainFunctionToggleMode.On : TrainFunctionToggleMode.Off
        })
        .ToArray();

      z21Client.SendBatchRequests(requests);
    }

    // GET: api/Trains/5
    [HttpGet("{id}")]
    public ActionResult<TrainWithInformation> GetTrain(int id) {
      try {
        return trainTracker.Get(id);
      } catch (KeyNotFoundException) {
        return NotFound();
      }
    }

    // PUT: api/Trains/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public IActionResult PutTrain(int id, Train train) {
      if (id != train.Id) {
        return BadRequest();
      }
      try {
        var existingValue = trainTracker.Get(id);
        existingValue.Name = train.Name;
      } catch (KeyNotFoundException) {
        return NotFound();
      }

      return NoContent();
    }

    // POST: api/Trains
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TrainWithInformation>> PostTrain(Train train) {
      var locoInformation = await z21Client.GetLocomotiveInformation(new LocomotiveInformationRequest { LocomotiveAddress = (short)train.Id });
      var newTrain = new TrainWithInformation { Id = train.Id, Name = train.Name, TrainFunctions = locoInformation.TrainFunctions, Speed = locoInformation.TrainSpeed };
      trainTracker.Add(train.Id, newTrain);
      return newTrain;
    }

    // DELETE: api/Trains/5
    [HttpDelete("{id}")]
    public IActionResult DeleteTrain(int id) {
      if (trainTracker.Remove(id)) {
        return NoContent();
      } else {
        return NotFound();
      }
    }
  }
}
