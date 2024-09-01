namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Spawning : Unmarked, IGet<Input.Spawned> {
      public Spawning() {
        this.OnEnter(() => Output(new Output.SpawnStarted()));
        this.OnExit(() => Output(new Output.SpawnComplete()));
      }

      public Transition On(in Input.Spawned input) => To<Idle>();
    }
  }
}
