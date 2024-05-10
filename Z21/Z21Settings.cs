using System;
using System.Net;

namespace Z21;
public class Z21Settings {
  public IPEndPoint Z21Endpoint { get; set; }
  public TimeSpan Timeout { get; set; }
}
