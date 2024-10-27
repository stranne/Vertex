namespace Vertex.InGameAudio;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using Vertex.Game.Domain;

public partial class InGameAudioLogic {
  [Meta]
  public partial record State : StateLogic<State> {
    public State() {
      OnAttach(() => {
        var gameRepo = Get<IGameRepo>();
        gameRepo.NewGame += OnNewGame;
        gameRepo.GameEnded += OnGameEnded;
        gameRepo.GridNodeSelected += OnGridNodeSelected;
      });

      OnDetach(() => {
        var gameRepo = Get<IGameRepo>();
        gameRepo.NewGame -= OnNewGame;
        gameRepo.GameEnded -= OnGameEnded;
        gameRepo.GridNodeSelected -= OnGridNodeSelected;
      });
    }

    public void OnNewGame() => Output(new Output.NewGame());

    public void OnGameEnded() => Output(new Output.GameEnded());

    public void OnGridNodeSelected(Vector2I _) => Output(new Output.GridNodeSelected());
  }
}
