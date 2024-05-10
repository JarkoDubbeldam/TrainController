using System.ComponentModel.DataAnnotations;

namespace Trains.DataAccess.Models;
public class Train {
  [Range(1, 128)]
  public int Id { get; set; }
  [Required]
  public required string Name { get; set; }
  public required string Icon { get; set; }
}
