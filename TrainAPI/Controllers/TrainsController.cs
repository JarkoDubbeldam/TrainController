using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainAPI.Data;
using TrainAPI.Models;
using Z21;

namespace TrainAPI.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class TrainsController : ControllerBase {
    private readonly TrainAPIContext _context;
    private readonly IZ21Client z21Client;

    public TrainsController(TrainAPIContext context, IZ21Client z21Client) {
      _context = context;
      this.z21Client = z21Client;
    }

    // GET: api/Trains
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Train>>> GetTrain([FromQuery] bool includeState = false) {
      var trains = await _context.Train.ToListAsync();
      if (includeState) {
        return await Task.WhenAll(trains.Select(IncludeCurrentInformation));
      }
      return trains;
    }

    // GET: api/Trains/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Train>> GetTrain(int id, [FromQuery] bool includeState = false) {
      var train = await _context.Train.FindAsync(id);

      if (train == null) {
        return NotFound();
      }

      if (includeState) {
        return await IncludeCurrentInformation(train);
      }

      return train;
    }

    private async Task<Train> IncludeCurrentInformation(Train train) {
      var result = await z21Client.GetLocomotiveInformation(new Z21.API.LocomotiveInformationRequest {
        LocomotiveAddress = (short)train.Id
      });
      return new TrainWithInformation {
        Id = train.Id,
        Name = train.Name,
        Speed = result.TrainSpeed,
        TrainFunctions = result.TrainFunctions
      };
    }

    // PUT: api/Trains/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTrain(int id, Train train) {
      if (id != train.Id) {
        return BadRequest();
      }

      _context.Entry(train).State = EntityState.Modified;

      try {
        await _context.SaveChangesAsync();
      } catch (DbUpdateConcurrencyException) {
        if (!await TrainExists(id)) {
          return NotFound();
        } else {
          throw;
        }
      }

      return NoContent();
    }

    // POST: api/Trains
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Train>> PostTrain(Train train) {
      _context.Train.Add(train);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetTrain", new { id = train.Id }, train);
    }

    // DELETE: api/Trains/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrain(int id) {
      var train = await _context.Train.FindAsync(id);
      if (train == null) {
        return NotFound();
      }

      _context.Train.Remove(train);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private Task<bool> TrainExists(int id) {
      return _context.Train.AnyAsync(e => e.Id == id);
    }
  }
}
