using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Z21;
public class Z21Settings {
  public IPEndPoint Z21Endpoint { get; set; }
  public TimeSpan Timeout{ get; set; }
}
