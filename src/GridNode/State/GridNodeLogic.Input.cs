namespace Vertex.GridNode.State;

public partial class GridNodeLogic {
  public abstract partial record Input {
    public readonly record struct Spawned;
    public readonly record struct HoverEnter;
    public readonly record struct HoverExit;
    public readonly record struct Selected(int PlayerId);

    public readonly record struct WinningLine;
    public readonly record struct GameOver;
    public readonly record struct Restart;
  }
}
