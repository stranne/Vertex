namespace Vertex.GridNode.State;

public partial class GridNodeLogic {
  public abstract partial record Output {
    public readonly record struct SpawnStarted;
    public readonly record struct SpawnComplete;
    public readonly record struct HoverEntered;
    public readonly record struct HoverExited;
    public readonly record struct Disabled;

    public readonly record struct Selected(int PlayerId);
    public readonly record struct Marked(int PlayerId);
  }
}
