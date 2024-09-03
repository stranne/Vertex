namespace Vertex.Game.State;

using System.Collections.Generic;
using Vertex.GridNode;

public partial class GameLogic {
  public abstract partial record Output {
    public readonly record struct Starting;
    public readonly record struct Ending;
    public readonly record struct AddNewGridNodes(List<IGridNode> GridNodes);
  }
}
