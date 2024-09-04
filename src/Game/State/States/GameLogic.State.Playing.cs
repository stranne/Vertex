namespace Vertex.Game.State;

using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode;

public partial class GameLogic {
  public abstract partial record State {
    [Meta]
    public partial record Playing : State, IGet<Input.GameOver> {
      public Playing() {
        OnAttach(() => {
          var gameRepo = Get<IGameRepo>();
          gameRepo.PopulateGridPositions += OnPopulateGridPositions;
          gameRepo.GameEnded += OnGameEnded;
        });

        OnDetach(() => {
          var gameRepo = Get<IGameRepo>();
          gameRepo.PopulateGridPositions -= OnPopulateGridPositions;
          gameRepo.GameEnded += OnGameEnded;
        });

        this.OnEnter(() => Output(new Output.NewGame()));
      }

      public Transition On(in Input.GameOver input) => To<GameOver>();

      public void OnPopulateGridPositions(List<Vector2I> gridPositions) =>
        Output(new Output.AddNewGridNodes(gridPositions));

      public void OnGameEnded(List<Vector2I> _) => To<GameOver>();
    }
  }
}
