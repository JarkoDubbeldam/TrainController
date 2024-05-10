using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Z21;
public class Z21Settings {
  [Required]
  public required IPEndPoint Z21Endpoint { get; set; }
  public TimeSpan Timeout { get; set; }
}
