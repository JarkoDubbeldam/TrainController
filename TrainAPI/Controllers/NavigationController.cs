using Microsoft.AspNetCore.Mvc;
using TrainAPI.Data;
using TrainAPI.Trackers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Z21;
using Z21.API;

namespace TrainAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NavigationController : ControllerBase {
  private readonly INavigator navigator;
  private readonly TrainAPIContext trainAPIContext;
  private readonly ITrainPositionTracker trainPositionTracker;
  private readonly IZ21Client z21Client;

  public NavigationController(INavigator navigator, TrainAPIContext trainAPIContext, ITrainPositionTracker trainPositionTracker, IZ21Client z21Client) {
    this.navigator = navigator;
    this.trainAPIContext = trainAPIContext;
    this.trainPositionTracker = trainPositionTracker;
    this.z21Client = z21Client;
  }


  [HttpPost]
  public async Task Nagivate([FromBody] NavigationRequest navigationRequest) {
    var fromPosition = trainPositionTracker.GetTrainPosition(navigationRequest.TrainId);
    var route = await FindRoute(fromPosition.OccupiedTrackSections.First.Value, navigationRequest.TargetSectionId);
    _ = navigator.Navigate(navigationRequest.TrainId, fromPosition.LastSeenDirection, route);
  }

  private async Task<List<int>> FindRoute(int from, int to) {
    var connection = (SqlConnection)trainAPIContext.Database.GetDbConnection();
    using var command = new SqlCommand("exec dbo.FindRoute @from, @to", connection);
    command.Parameters.Add("@from", System.Data.SqlDbType.Int).Value = from;
    command.Parameters.Add("@to", System.Data.SqlDbType.Int).Value = to;
    await connection.OpenAsync();
    try {
      var result = await command.ExecuteReaderAsync();
      if (await result.ReadAsync()) {
        return result.GetString(0).Split(',').Select(int.Parse).ToList();
      }
      throw new KeyNotFoundException();
    } finally {
      await connection.CloseAsync();
    }
  }

  public class NavigationRequest {
    public int TrainId { get; set; }
    public int TargetSectionId { get; set; }
  }
}
