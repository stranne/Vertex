namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record MarkedDisabled : Marked {
      public MarkedDisabled() {
        this.OnEnter(() => Output(new Output.Disabled()));
      }
    }
  }
}
