namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Marked : State, IGet<Input.GameOver>, IGet<Input.InWinningLine>, IGet<Input.Restart> {
      public Marked() {
        this.OnEnter(() => Output(new Output.Selected(Get<Data>().Color!.Value)));
      }

      public Transition On(in Input.GameOver input) => To<MarkedDisabled>();

      public Transition On(in Input.InWinningLine input) => To<WinningNode>();

      public Transition On(in Input.Restart input) => To<Spawning>();
    }
  }
}
