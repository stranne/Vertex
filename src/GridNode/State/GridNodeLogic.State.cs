namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  [Meta]
  public abstract partial record State : StateLogic<State> {

  }
}
