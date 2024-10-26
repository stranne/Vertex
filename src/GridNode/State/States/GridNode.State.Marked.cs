namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Marked : State, IGet<Input.GameOver>, IGet<Input.InWinningLine> {
      public Marked() {
        this.OnEnter(() => {
          var data = Get<Data>();
          Output(new Output.Selected(data.Color!.Value));
        });
      }

      public Transition On(in Input.GameOver input) => To<MarkedDisabled>();

      public Transition On(in Input.InWinningLine input) {
        var lineIndex = input.LineIndex;
        var lineLength = input.LineLength;

        return To<WinningNode>().With(
          (state) => {
            var winningNode = (WinningNode)state;
            winningNode.LineIndex = lineIndex;
            winningNode.LineLength = lineLength;
          });
      }
    }
  }
}
