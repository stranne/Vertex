namespace Vertex.Game.State;

public partial class GameLogic {
  public abstract partial record Input {
    public readonly record struct StartGame;
    public readonly record struct GameOver;
    public readonly record struct Restart;
  }
}
