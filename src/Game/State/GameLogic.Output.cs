namespace Vertex.Game.State;

using System.Collections.Generic;
using Chickensoft.GodotNodeInterfaces;
using Godot;
using Vertex.GridNode;

public partial class GameLogic {
  public abstract partial record Output {
    public readonly record struct NewGame;
    public readonly record struct AddNewGridNode(IGridNode GridNode);
    public readonly record struct Ending;
    public readonly record struct Restart;
  }
}
