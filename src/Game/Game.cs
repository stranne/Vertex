namespace Vertex.Game;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.CameraRig;
using Vertex.Game.Domain;
using Vertex.Game.State;
using Vertex.Menu.GameEnded;
using Vertex.Menu.Start;

public interface IGame : INode3D, IProvide<IGameRepo>, IProvide<IGridNodeMediator>, IProvide<IGridBounds> { }

[Meta(typeof(IAutoNode))]
public partial class Game : Node3D, IGame {
  public override void _Notification(int what) => this.Notify(what);

  #region States
  public IGameLogic GameLogic { get; set; } = default!;
  public GameLogic.IBinding GameLogicBinding { get; set; } = default!;

  public IGameRepo GameRepo { get; set; } = default!;
  IGameRepo IProvide<IGameRepo>.Value() => GameRepo;

  public IGridNodeMediator GridNodeMediator { get; set; } = default!;
  IGridNodeMediator IProvide<IGridNodeMediator>.Value() => GridNodeMediator;

  public IGridBounds GridBounds { get; set; } = default!;
  IGridBounds IProvide<IGridBounds>.Value() => GridBounds;

  [Export]
  public Color[] PlayerColors = [
    new(1, 0, 0),
    new(0, 1, 0),
  ];

  [Export]
  public PackedScene GridNodeScene = default!;
  #endregion

  #region Nodes
  [Node]
  public ICameraRig CameraRig { get; set; } = default!;

  [Node]
  public INode3D GridBoard { get; set; } = default!;

  [Node]
  public IStartMenu StartMenu { get; set; } = default!;

  [Node]
  public IGameEndedMenu GameEndedMenu { get; set; } = default!;
  #endregion

  public void Setup() {
    GridNodeMediator = new GridNodeMediator(GridNodeScene);
    GameRepo = new GameRepo(PlayerColors, GridNodeMediator);
    GridBounds = new GridBounds();

    StartMenu.StartGame += OnStartGame;
    GameEndedMenu.Restart += OnRestart;
    GameRepo.GameEnded += OnGameEnded;
    GameRepo.GridNodeSelected += OnGridNodeSelected;

    this.Provide();
  }

  public void OnExitTree() {
    GameLogic.Stop();
    GameLogicBinding.Dispose();

    StartMenu.StartGame -= OnStartGame;
    GameEndedMenu.Restart -= OnRestart;
    GameRepo.GameEnded -= OnGameEnded;
    GameRepo.GridNodeSelected -= OnGridNodeSelected;
  }

  public void OnResolved() {
    GameLogic = new GameLogic();
    GameLogic.Set(GameRepo);
    GameLogic.Set(GridNodeMediator);

    GameLogicBinding = GameLogic.Bind();

    GameLogicBinding
      .Handle((in GameLogic.Output.NewGame _) => {
        StartMenu.Visible = false;
        GameEndedMenu.Visible = false;
        GameRepo.StartNewGame();
        GridBounds.Reset();
      })
      .Handle((in GameLogic.Output.AddNewGridNode output) => {
        GameRepo.AddGridNode(output.GridPosition);
        GridBoard.AddChild((Node3D)output.GridNode);
      })
      .Handle((in GameLogic.Output.Ending output) => {
        // TODO view game over screen
        GameEndedMenu.Visible = true;
      });

    GameLogic.Start();
  }

  public void OnStartGame() =>
    GameLogic.Input(new GameLogic.Input.StartGame());

  public void OnRestart() =>
    GameLogic.Input(new GameLogic.Input.Restart());

  public void OnGameEnded() =>
    GameLogic.Input(new GameLogic.Input.GameEnded());

  public void OnGridNodeSelected(Vector2I gridPosition) =>
    GridBounds.UpdateBounds(gridPosition);

  private static float CalculateZoomFactor(float gridWidth, float gridHeight, float screenWidth, float screenHeight) {
    var zoomX = screenWidth / gridWidth;
    var zoomY = screenHeight / gridHeight;

    return Mathf.Max(zoomX, zoomY);
  }
}
