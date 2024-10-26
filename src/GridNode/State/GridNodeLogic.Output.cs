namespace Vertex.GridNode.State;

using Godot;

public partial class GridNodeLogic {
  public abstract partial record Output {
    public readonly record struct Spawn;
    public readonly record struct HoverEntered(Color Color);
    public readonly record struct HoverExited;
    public readonly record struct Disabled;

    public readonly record struct Selected(Color Color);
    public readonly record struct Winning(int LineIndex, int LineLength);
  }
}
