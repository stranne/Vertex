namespace Vertex.GridNode.State;

public partial class GridNodeLogic {
  public abstract partial record Output {
    public readonly record struct SpawnStarted;
    public readonly record struct SpawnComplete;
  }
}
