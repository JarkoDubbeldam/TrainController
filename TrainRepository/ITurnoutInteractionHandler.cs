using Z21.Domain;

namespace TrainRepository {
  public interface ITurnoutInteractionHandler {
    void SetTurnoutPosition(int address, TurnoutPosition turnoutPosition);
  }
}