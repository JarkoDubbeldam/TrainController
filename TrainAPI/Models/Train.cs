using Z21.Domain;

namespace TrainAPI.Models;

public class Train {
  public int Id { get; set; }
  public string Name { get; set; }
}

public class TrainWithInformation : Train {
  public TrainSpeed Speed { get; set; }
  public TrainFunctions TrainFunctions { get; set; }
}
