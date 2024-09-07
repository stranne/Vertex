namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Idle : Unmarked, IGet<Input.HoverEnter> {
      public Transition On(in Input.HoverEnter input) {
        Get<Data>().Color = input.Color;
        return To<Hover>();
      }
    }
  }
}
