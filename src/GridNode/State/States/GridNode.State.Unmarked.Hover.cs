namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using Vertex.Game.Domain;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Hover : Unmarked, IGet<Input.HoverExit>, IGet<Input.Select> {
      public required Color Color { get; init; }

      public Hover() {
        this.OnEnter(() => Output(new Output.HoverEntered(Color)));
        this.OnExit(() => Output(new Output.HoverExited()));
      }

      public Transition On(in Input.HoverExit input) => To<Idle>();

      public Transition On(in Input.Select input) => To<Marked>();
    }
  }
}
