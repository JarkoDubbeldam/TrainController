
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainRepository {
  public class TurnoutRepository : Repository<Turnout>, IDisposable {
    private readonly IDisposable subscription;
    private readonly ITurnoutInteractionHandler turnoutHandler;

    public TurnoutRepository(IZ21Client z21Client, ITurnoutInteractionHandler turnoutHandler) : base(z21Client) {
      subscription = z21Client.TurnoutInformationChanged.Subscribe(x => Z21Client_LocomotiveInformationChanged(x));
      this.turnoutHandler = turnoutHandler;
    }

    private void Z21Client_LocomotiveInformationChanged(TurnoutInformation e) {
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

    public override void Dispose() {
      subscription.Dispose();
    }
  }
}

