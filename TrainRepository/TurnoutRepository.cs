
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  public class TurnoutRepository : Repository<Turnout> {
    private readonly ITurnoutInteractionHandler turnoutHandler;

    public TurnoutRepository(IZ21Client z21Client, ITurnoutInteractionHandler turnoutHandler) : base(z21Client) {

      z21Client.TurnoutInformationChanged += Z21Client_LocomotiveInformationChanged;
      this.turnoutHandler = turnoutHandler;
    }

    private void Z21Client_LocomotiveInformationChanged(object sender, TurnoutInformation e) {
      repos.TryGetValue(e.Address, out var turnout);
      if (turnout.IsCompletedSuccessfully) {
        turnout.Result.Update(e);
      }
    }



    protected override async Task<Turnout> GetObjectInfoFromController(int address, string name) {
      var request = new TurnoutInformationRequest { Address = (short)address };
      var response = await client.Value.GetTurnoutInformation(request);

      var turnout = new Turnout(address, name, response.TurnoutPosition);
      return turnout;
    }

    protected override void ObjectChangedByUserHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      switch (e.PropertyName) {
        case nameof(Turnout.TurnoutPosition): {
            var turnout = (Turnout)sender;
            turnoutHandler.SetTurnoutPosition(turnout.Address, turnout.TurnoutPosition);
          }
          return;
      }

    }
  }
}

