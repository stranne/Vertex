namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Marked : State, IGet<Input.GameOver>, IGet<Input.WinningNode>, IGet<Input.Restart> {
      public int SelectedPlayerId { get; set; } = default!;

      public Marked() {
        this.OnEnter(() => Output(new Output.Marked(SelectedPlayerId)));
      }

      public Transition On(in Input.GameOver input) => To<MarkedDisabled>();

      public Transition On(in Input.WinningNode input) => To<WinningNode>();

      public Transition On(in Input.Restart input) => To<Spawning>();
    }
  }
}
