namespace Vertex.Game;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.Game.State;
using Vertex.GridNode;

public interface IGame : INode3D, IProvide<IGameRepo> { }

[Meta(typeof(IAutoNode))]
public partial class Game : Node3D, IGame {
  public override void _Notification(int what) => this.Notify(what);

  #region States
  public IGameLogic GameLogic { get; set; } = default!;
  public GameLogic.IBinding GameLogicBinding { get; set; } = default!;

  public IGameRepo GameRepo { get; set; } = default!;
  IGameRepo IProvide<IGameRepo>.Value() => GameRepo;

  [Export]
  public Color[] PlayerColors = [
    new(1, 0, 0),
    new(0, 1, 0),
  ];
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
  #endregion

  public void Setup() {
    GameRepo = new GameRepo(PlayerColors);

    this.Provide();
  }

  public void OnResolved() {
    GameLogic = new GameLogic();
    GameLogic.Set(GameRepo);

    GameLogicBinding = GameLogic.Bind();

    GameLogicBinding
      .Handle((in GameLogic.Output.NewGame _) =>
        GameRepo.StartNewGame()
      )
      .Handle((in GameLogic.Output.AddNewGridNodes output) => {
        // output.GridPositions.ForEach(gridNode => GridBoard.AddChild((Node3D)gridNode))
      })
      .Handle((in GameLogic.Output.Ending _) => {
        // TODO view game over screen
      });

    GameLogic.Start();
  }

  public override void _Process(double delta) => HandleGridNodeHoverAndClick();

  public void HandleGridNodeHoverAndClick() {
    var mousePosition = GetViewport().GetMousePosition();
    var from = Camera.ProjectRayOrigin(mousePosition);
    var to = Camera.ProjectRayNormal(mousePosition) * Camera.Position.Z * 2;

    RayCast.GlobalPosition = from;
    RayCast.TargetPosition = to - from;

    RayCast.ForceRaycastUpdate();
    var collidedNode = RayCast.GetCollider() as Node;
    var gridPosition = collidedNode?.GetParent()?.GetParent<IGridNode>()?.GridPosition;
    var isLeftMouseButtonPressed = Input.IsActionJustPressed("mouse_left_click");
    GameRepo.MouseEvent(gridPosition, isLeftMouseButtonPressed);
  }
}
