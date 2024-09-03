namespace Vertex.Game.State;

using System.Collections.Generic;
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
          var gameRepo = Get<IGameRepo>();
          gameRepo.AddNewGridNodes += OnAddNewGridNodes;
        });

        OnDetach(() => {
          var gameRepo = Get<IGameRepo>();
          gameRepo.AddNewGridNodes -= OnAddNewGridNodes;
        });

        this.OnEnter(() => Output(new Output.Starting()));
      }

      public Transition On(in Input.GameOver input) => To<GameOver>();

      public void OnAddNewGridNodes(List<IGridNode> gridNodes) =>
        Output(new Output.AddNewGridNodes(gridNodes));
    }
  }
}
