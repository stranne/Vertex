namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Hover : Unmarked, IGet<Input.HoverExit>, IGet<Input.Select> {
      public Hover() {
        this.OnEnter(() => {
          var data = Get<Data>();
          Output(new Output.HoverEntered { Color = data.Color!.Value });
        });
        this.OnExit(() => Output(new Output.HoverExited()));
      }

      public Transition On(in Input.HoverExit input) {
        Get<Data>().Color = null;
        return To<Idle>();
      }

      public Transition On(in Input.Select input) => To<Marked>();
    }
  }
}
