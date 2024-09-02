namespace Vertex.GridNode.State;

public partial class GridNodeLogic {
  public abstract partial record Input {
    #region Unmarked
    public readonly record struct Spawned;
    public readonly record struct HoverEnter;
    public readonly record struct HoverExit;
    public readonly record struct Select(int PlayerId);
    #endregion

    #region Marked
    public readonly record struct InWinningLine;
    public readonly record struct GameOver;
    public readonly record struct Restart;
    #endregion
  }
}
