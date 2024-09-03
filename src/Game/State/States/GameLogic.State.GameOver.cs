namespace Vertex.Game.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record GameOver : State, IGet<Input.Restart> {
      public GameOver() {
        this.OnEnter(() => Output(new Output.Ending()));
      }

      public Transition On(in Input.Restart input) => To<Playing>();
    }
  }
}
