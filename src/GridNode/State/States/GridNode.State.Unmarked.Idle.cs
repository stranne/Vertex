namespace Vertex.GridNode.State;

using Chickensoft.Introspection;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Idle : Unmarked, IGet<Input.HoverEnter> {
      public Transition On(in Input.HoverEnter input) => To<Hover>();
    }
  }
}
