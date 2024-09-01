namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Hover : Unmarked, IGet<Input.HoverExit> {
      public Hover() {
        this.OnEnter(() => Output(new Output.HoverEntered()));
        this.OnExit(() => Output(new Output.HoverExited()));
      }

      public Transition On(in Input.HoverExit input) => To<UnmarkedIdle>();
    }
  }
}
