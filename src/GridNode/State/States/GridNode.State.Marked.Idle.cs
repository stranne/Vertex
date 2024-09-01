namespace Vertex.GridNode.State;

using Chickensoft.Introspection;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record MarkedIdle : Marked {
      public Transition On(in Input.HoverEnter _) => To<Hover>();
    }
  }
}
