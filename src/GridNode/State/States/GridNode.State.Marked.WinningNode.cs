namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record WinningNode : Marked {
      public int LineIndex { get; set; }
      public int LineLength { get; set; }

      public WinningNode() {
        this.OnEnter(() => {
          var data = Get<Data>();
          Output(new Output.Winning(LineIndex, LineLength));
        });
      }
    };
  }
}
