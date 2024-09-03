namespace Vertex.Game.State;

using Chickensoft.Introspection;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record Menu : State, IGet<Input.StartGame> {
      public Transition On(in Input.StartGame input) => To<Playing>();
    }
  }
}
