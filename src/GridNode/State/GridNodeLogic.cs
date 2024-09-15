namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public interface IGridNodeLogic : ILogicBlock<GridNodeLogic.State> { }

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class GridNodeLogic : LogicBlock<GridNodeLogic.State>, IGridNodeLogic {
  public override Transition GetInitialState() => To<State.Idle>();
}
