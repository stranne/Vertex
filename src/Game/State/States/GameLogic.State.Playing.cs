namespace Vertex.Game.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Vertex.Game.Domain;
using Vertex.GridNode;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record Playing : State, IGet<Input.GameOver> {
      public Playing() {
        OnAttach(() => {
          var gridNodeMediator = Get<IGridNodeMediator>();
          gridNodeMediator.AddNewGridNode += OnAddNewGridNode;
        });
        OnDetach(() => {
          var gridNodeMediator = Get<IGridNodeMediator>();
          gridNodeMediator.AddNewGridNode -= OnAddNewGridNode;
        });
        this.OnEnter(() => Output(new Output.NewGame()));
      }

      public Transition On(in Input.GameOver input) => To<GameOver>();

      public void OnAddNewGridNode(IGridNode gridNode) =>
        Output(new Output.AddNewGridNode(gridNode));
    }
  }
}
