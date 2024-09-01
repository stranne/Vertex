namespace Vertex.GridNode.State;

using Chickensoft.Introspection;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Unmarked : State, IGet<Input.Select>, IGet<Input.GameOver>, IGet<Input.Restart> {
      public Transition On(in Input.Select input) {
        var playerId = input.PlayerId;
        return To<Marked>().With(state => ((Marked)state).SelectedPlayerId = playerId);
      }

      public Transition On(in Input.GameOver input) => To<UnmarkedDisabled>();

      public Transition On(in Input.Restart input) => To<Spawning>();
    }
  }
}
