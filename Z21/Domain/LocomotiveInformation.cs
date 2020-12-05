using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Z21.Domain {
  public class LocomotiveInformation {
    public short Address { get; set; }
    public bool IsBusy { get; set; }
    public TrainSpeed TrainSpeed { get; set; }
    public TrainFunctions TrainFunctions { get; set; }

    public override string ToString() {
      return $"Train {Address}\n{TrainSpeed}\n{TrainFunctions}";
    }
  }

  [Flags]
  public enum TrainFunctions : uint {
    None = 0,
    DoubleTraction = 0x00000040,
    Smartsearch = 0x00000020,
    Lights = 0x00000010,
    Function1 = 0x00000001,
    Function2 = 0x00000002,
    Function3 = 0x00000004,
    Function4 = 0x00000008,
    Function5 = 0x00000100,
    Function6 = 0x00000200,
    Function7 = 0x00000400,
    Function8 = 0x00000800,
    Function9 = 0x00001000,
    Function10 = 0x00002000,
    Function11 = 0x00004000,
    Function12 = 0x00008000,
    Function13 = 0x00010000,
    Function14 = 0x00020000,
    Function15 = 0x00040000,
    Function16 = 0x00080000,
    Function17 = 0x00100000,
    Function18 = 0x00200000,
    Function19 = 0x00400000,
    Function20 = 0x00800000,
    Function21 = 0x01000000,
    Function22 = 0x02000000,
    Function23 = 0x04000000,
    Function24 = 0x08000000,
    Function25 = 0x10000000,
    Function26 = 0x20000000,
    Function27 = 0x40000000,
    Function28 = 0x80000000
  }

}