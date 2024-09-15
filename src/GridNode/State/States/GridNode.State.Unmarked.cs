namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;


public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Unmarked : State, IGet<Input.GameOver>, IGet<Input.Reset> {
      public Unmarked() {
        this.OnEnter(() => Output(new Output.Spawn()));
      }

      public Transition On(in Input.GameOver input) => To<UnmarkedDisabled>();

      public Transition On(in Input.Reset input) => ToSelf();
    }
  }
}
