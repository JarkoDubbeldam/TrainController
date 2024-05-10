namespace Z21.Domain {
  public class TurnoutInformation {
    public short Address { get; set; }
    public TurnoutPosition TurnoutPosition { get; set; }

    public override string ToString() {
      return $"Turnout {Address}\n{TurnoutPosition}";
    }
  }
}
