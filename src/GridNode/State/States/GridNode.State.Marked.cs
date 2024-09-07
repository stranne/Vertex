namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Marked : State, IGet<Input.GameOver>, IGet<Input.InWinningLine>, IGet<Input.Reset> {
      public Marked() {
        this.OnEnter(() => {
          var data = Get<Data>();
          Output(new Output.Selected(data.Color!.Value));
        });
      }

      public Transition On(in Input.GameOver input) => To<MarkedDisabled>();

      public Transition On(in Input.InWinningLine input) => To<WinningNode>();

      public Transition On(in Input.Reset input) => To<Spawning>();
    }
  }
}
