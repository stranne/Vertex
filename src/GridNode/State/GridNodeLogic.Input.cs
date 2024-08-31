namespace Vertex.GridNode.State;

public partial class GridNodeLogic {
  public abstract partial record Input {
    public static class Hovering {
      public readonly record struct Enter(int PlayerId);
      public readonly record struct Idle;
      public readonly record struct Leave;
    }

    public readonly record struct Spawned;
    public readonly record struct Selected(int PlayerId);

    public readonly record struct WinningLine;
    public readonly record struct GameOver;
    public readonly record struct Restart;
  }
}
