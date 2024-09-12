namespace Vertex.CameraRig;

using Chickensoft.AutoInject;
using Chickensoft.GoDotLog;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode;

public interface ICameraRig : INode3D { }

[Meta(typeof(IAutoNode))]
public partial class CameraRig : Node3D, ICameraRig {
  public override void _Notification(int what) => this.Notify(what);

  private readonly GDLog _log = new(nameof(CameraRig));

  // private Tween _tween = default!;

  [Dependency]
  public IGameRepo GameRepo => this.DependOn<IGameRepo>();

  [Dependency]
  public IGridBounds GridBounds => this.DependOn<IGridBounds>();

  [Node]
  public ICamera3D Camera { get; set; } = default!;

  [Node]
  public IRayCast3D RayCast { get; set; } = default!;

  public void OnReady() {
    // _tween = GetTree().CreateTween();
  }

  public void OnResolved() {
    GridBounds.BoundsUpdated += OnBoundsUpdated;
    GameRepo.NewGame += OnEnableRayCast;
    GameRepo.GameEnded += OnDisableRayCast;
  }

  public override void _Process(double delta) => HandleGridNodeHoverAndClick();

  public override void _PhysicsProcess(double delta) => UpdateRayCast();

  public void ExitTree() {
    GridBounds.BoundsUpdated -= OnBoundsUpdated;
    GameRepo.NewGame -= OnEnableRayCast;
    GameRepo.GameEnded -= OnDisableRayCast;
  }

  public void OnBoundsUpdated(int minX, int maxX, int minY, int maxY) {
    const float borderMargin = 1.2f;

    var bounds = new Rect2(minX, minY, maxX - minX, maxY - minY);

    var boardWidth = bounds.Size.X + (borderMargin * 2);
    var boardHeight = bounds.Size.Y + (borderMargin * 2);
    var gameBoardAspectRatio = boardWidth / boardHeight;
    var viewportAspectRatio = GetViewport().GetVisibleRect().Size.X /
                              GetViewport().GetVisibleRect().Size.Y;

    var orthogonalSize = viewportAspectRatio > gameBoardAspectRatio
      ? boardHeight
      : boardWidth / viewportAspectRatio;

    Camera.Size = orthogonalSize;

    var center = bounds.Position + (bounds.Size / 2);
    var height = orthogonalSize * 2;
    Position = new Vector3(center.X, height, center.Y);
  }

  public void OnEnableRayCast() {
    _log.Print("Enabling RayCast");
    RayCast.Enabled = true;
  }

  public void OnDisableRayCast() {
    _log.Print("Disabling RayCast");
    RayCast.Enabled = false;
  }

  public void UpdateRayCast() {
    if (!RayCast.Enabled) {
      return;
    }

    var mousePosition = GetViewport().GetMousePosition();
    var from = Camera.ProjectRayOrigin(mousePosition);
    var to = Camera.ProjectRayNormal(mousePosition) * 1000;

    RayCast.GlobalPosition = from;
    RayCast.TargetPosition = to - from;
  }

  public void HandleGridNodeHoverAndClick() {
    if (!RayCast.Enabled) {
      return;
    }

    var collidedNode = RayCast.GetCollider() as Node;
    var gridNode = GetClosestParent<IGridNode>(collidedNode);
    var isLeftMouseButtonPressed = Input.IsActionJustPressed("mouse_left_click");
    GameRepo.MouseEvent(gridNode, isLeftMouseButtonPressed);
  }

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