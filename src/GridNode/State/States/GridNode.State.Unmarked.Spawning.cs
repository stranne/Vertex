namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Spawning : Unmarked {
      public Spawning() {
        this.OnEnter(() => Output(new Output.SpawnStarted()));
      }

      public Transition On(in Input.Spawned input) => To<UnmarkedIdle>();
    }
  }
}
