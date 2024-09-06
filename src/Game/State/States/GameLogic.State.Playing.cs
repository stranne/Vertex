namespace Vertex.Game.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record Playing : State, IGet<Input.GameOver> {
      public Playing() {
        this.OnEnter(() => Output(new Output.NewGame()));
      }

      public Transition On(in Input.GameOver input) => To<GameOver>();
    }
  }
}
