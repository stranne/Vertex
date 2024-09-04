namespace Vertex.GridNode.State;

using Godot;

public partial class GridNodeLogic {
  public abstract partial record Output {
    public readonly record struct SpawnStarted;
    public readonly record struct SpawnComplete;
    public readonly record struct HoverEntered(Color Color);
    public readonly record struct HoverExited;
    public readonly record struct Disabled;

    public readonly record struct Selected(Color Color);
  }
}
