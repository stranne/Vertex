namespace Vertex.GridNode.State;

using Chickensoft.Introspection;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Idle : Unmarked, IGet<Input.HoverEnter>, IGet<Input.Select> {
      public Transition On(in Input.HoverEnter input) => To<Hover>();

      public Transition On(in Input.Select input) {
        var playerId = input.PlayerId;
        return To<Marked>().With(state => ((Marked)state).SelectedPlayerId = playerId);
      }
    }
  }
}
