namespace Vertex.GridNode.State;

using Godot;

public partial class GridNodeLogic {
  public record Data {
    public required Vector2I GridPosition { get; init; }
    public Color? Color { get; set; }
  };
}
