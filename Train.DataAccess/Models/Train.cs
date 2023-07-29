using System.ComponentModel.DataAnnotations;

namespace Trains.DataAccess.Models;
public class Train {
  [Range(1, 128)]
  public int Id { get; set; }
  [Required]
  public string Name { get; set; }
  public string Icon { get; set; }
}
