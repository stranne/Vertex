namespace Vertex.Game.State;

using System.Collections.Generic;
using Godot;

public partial class GameLogic {
  public abstract partial record Output {
    public readonly record struct NewGame;
    public readonly record struct AddNewGridNodes(List<Vector2I> GridPositions);
    public readonly record struct Ending;
  }
}
