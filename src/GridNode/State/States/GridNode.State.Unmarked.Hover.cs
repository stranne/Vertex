namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Hover : Unmarked, IGet<Input.HoverExit>, IGet<Input.Select> {
      public Hover() {
        this.OnEnter(() => Output(new Output.HoverEntered()));
        this.OnExit(() => Output(new Output.HoverExited()));
      }

      public Transition On(in Input.HoverExit input) => To<Idle>();

      public Transition On(in Input.Select input) {
        var playerId = input.PlayerId;
        return To<Marked>().With(state => ((Marked)state).SelectedPlayerId = playerId);
      }
    }
  }
}
