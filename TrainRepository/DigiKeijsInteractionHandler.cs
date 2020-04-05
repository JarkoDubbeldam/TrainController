using System;
using System.Collections.Generic;
using System.Text;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  public class DigiKeijsInteractionHandler : ITurnoutInteractionHandler {
    private readonly IZ21Client z21Client;

    public DigiKeijsInteractionHandler(IZ21Client z21Client) {
      this.z21Client = z21Client;
    }

    public void SetTurnoutPosition(int address, TurnoutPosition turnoutPosition) {
      var request = new SetTurnoutRequest {
        Address = (short)address,
        TurnoutPosition = turnoutPosition,
        Activation = Activation.Activate,
        QueueMode = true
      };
      z21Client.SetTurnout(request);
    }
  }
}
