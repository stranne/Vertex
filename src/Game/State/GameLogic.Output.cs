namespace Vertex.Game.State;

using Godot;
using Vertex.GridNode;

public partial class GameLogic {
  public abstract partial record Output {
    public readonly record struct NewGame;
    public readonly record struct AddNewGridNode(Vector2I GridPosition, IGridNode GridNode);
    public readonly record struct Ending;
  }
}
