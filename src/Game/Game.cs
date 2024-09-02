namespace Vertex.Game;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode;

public interface IGame : INode3D, IProvide<IGameRepo> { }

[Meta(typeof(IAutoNode))]
public partial class Game : Node3D, IGame {
  public override void _Notification(int what) => this.Notify(what);

  public IGameRepo GameRepo { get; set; } = default!;
  IGameRepo IProvide<IGameRepo>.Value() => GameRepo;

  [Export]
  public Color[] PlayerColors = [
    new(1, 0, 0),
    new(0, 1, 0),
  ];

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

  public override void _Process(double delta) => HandleGridNodeMouseEvents();

  public void HandleGridNodeMouseEvents() {
    var mousePosition = GetViewport().GetMousePosition();
    var from = Camera.ProjectRayOrigin(mousePosition);
    var to = Camera.ProjectRayNormal(mousePosition) * 1000;

    RayCast.GlobalPosition = from;
    RayCast.TargetPosition = to - from;

    RayCast.ForceRaycastUpdate();
    var gridNode = (RayCast.GetCollider() as Node)?.GetParent()?.GetParent<IGridNode>();
    var isLeftMouseButtonPressed = Input.IsActionJustPressed("mouse_left_click");
    GameRepo.MouseEvent(gridNode, isLeftMouseButtonPressed);
  }
}
