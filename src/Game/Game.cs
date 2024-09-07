namespace Vertex.Game;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.Game.State;
using Vertex.GridNode;
using Vertex.Menu.GameEnded;
using Vertex.Menu.Start;


public interface IGame : INode3D, IProvide<IGameRepo>, IProvide<IGridNodeMediator> { }

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
  public ICamera3D Camera { get; set; } = default!;

  /// <remarks>
  /// RayCast has Enabled property set to false to avoid unnecessary ray casts in physics process.
  /// </remarks>
  [Node]
  public IRayCast3D RayCast { get; set; } = default!;

  [Node]
  public INode3D GridBoard { get; set; } = default!;

  [Node]
  public IMenuStart MenuStart { get; set; } = default!;

  [Node]
  public IMenuGameEnded MenuGameEnded { get; set; } = default!;
  #endregion

  public void Setup() {
    GridNodeMediator = new GridNodeMediator(GridNodeScene);
    GameRepo = new GameRepo(PlayerColors, GridNodeMediator);

    MenuStart.StartGame += OnStartGame;
    MenuGameEnded.Restart += OnRestart;

    this.Provide();
  }

  public void OnExitTree() {
    GameLogic.Stop();
    GameLogicBinding.Dispose();

    MenuStart.StartGame -= OnStartGame;
    MenuGameEnded.Restart -= OnRestart;
  }

  public void OnResolved() {
    GameLogic = new GameLogic();
    GameLogic.Set(GameRepo);
    GameLogic.Set(GridNodeMediator);

    GameLogicBinding = GameLogic.Bind();

    GameLogicBinding
      .Handle((in GameLogic.Output.NewGame _) => {
        MenuStart.Visible = false;
        GameRepo.StartNewGame();
      })
      .Handle((in GameLogic.Output.AddNewGridNode output) => {
        GameRepo.AddGridNode(output.GridPosition);
        GridBoard.AddChild((Node3D)output.GridNode);
      })
      .Handle((in GameLogic.Output.Ending output) => {
        // TODO view game over screen
        MenuGameEnded.Visible = true;
      })
      .Handle((in GameLogic.Output.Restart _) => {
        MenuGameEnded.Visible = false;
        // TODO restart
      });

    GameLogic.Start();
  }

  public override void _Process(double delta) => HandleGridNodeHoverAndClick();

  public void HandleGridNodeHoverAndClick() {
    var mousePosition = GetViewport().GetMousePosition();
    var from = Camera.ProjectRayOrigin(mousePosition);
    var to = Camera.ProjectRayNormal(mousePosition) * 1000;

    RayCast.GlobalPosition = from;
    RayCast.TargetPosition = to - from;

    RayCast.ForceRaycastUpdate();
    var collidedNode = RayCast.GetCollider() as Node;
    var gridNode = GetClosestParent<IGridNode>(collidedNode);
    var isLeftMouseButtonPressed = Input.IsActionJustPressed("mouse_left_click");
    GameRepo.MouseEvent(gridNode, isLeftMouseButtonPressed);
  }

  public void OnStartGame() => GameLogic.Input(new GameLogic.Input.StartGame());

  public void OnRestart() => GameLogic.Input(new GameLogic.Input.Restart());

  private static T? GetClosestParent<T>(Node? node) where T : class {
    while (node != null) {
      if (node is T targetNode) {
        return targetNode;
      }

      node = node.GetParent();
    }

    return null;
  }
}
