namespace Vertex.GridNode.State;

using Chickensoft.Introspection;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Unmarked : State, IGet<Input.GameOver>, IGet<Input.Restart> {
      public Transition On(in Input.GameOver input) => To<UnmarkedDisabled>();

      public Transition On(in Input.Restart input) => To<Spawning>();
    }
  }
}
