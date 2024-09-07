namespace Vertex.Game.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record GameEnded : State, IGet<Input.Restart> {
      public GameEnded() {
        this.OnEnter(() => Output(new Output.Ending()));
      }

      public Transition On(in Input.Restart input) => To<Playing>();
    }
  }
}
