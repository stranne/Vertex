namespace Vertex.GridNode.State;

using Godot;

public partial class GridNodeLogic {
  public record Data {
    public Vector2I GridPosition { get; set; }
    public Color? Color { get; set; }
  };
}
