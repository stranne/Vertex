namespace Vertex.Game.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record Playing : State, IGet<Input.GameEnded> {
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

      public Transition On(in Input.GameEnded input) => To<GameEnded>();

      public void OnAddNewGridNode(Vector2I gridPosition, IGridNode gridNode) =>
        Output(new Output.AddNewGridNode(gridPosition, gridNode));
    }
  }
}
