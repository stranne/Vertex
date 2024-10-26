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

  [Dependency]
  public IGameRepo GameRepo => this.DependOn<IGameRepo>();

  [Dependency]
  public IGridBounds GridBounds => this.DependOn<IGridBounds>();

  [Node]
  public Camera3D Camera { get; set; } = default!;

  [Node]
  public IRayCast3D RayCast { get; set; } = default!;

  public void OnResolved() {
    GameRepo.NewGame += OnEnableRayCast;
    GameRepo.GameEnded += OnDisableRayCast;
    GridBounds.BoundsUpdated += UpdateSizeAndPosition;
    GetViewport().SizeChanged += UpdateSizeAndPosition;
  }

  public override void _PhysicsProcess(double delta) => UpdateRayCast();

  public override void _Process(double delta) => HandleGridNodeHoverAndClick();

  public void ExitTree() {
    GameRepo.NewGame -= OnEnableRayCast;
    GameRepo.GameEnded -= OnDisableRayCast;
    GridBounds.BoundsUpdated -= UpdateSizeAndPosition;
    GetViewport().SizeChanged -= UpdateSizeAndPosition;
  }

  public void UpdateSizeAndPosition() {
    const float borderMargin = 1.2f;

    var bounds = new Rect2(GridBounds.MinX, GridBounds.MinY, GridBounds.MaxX - GridBounds.MinX, GridBounds.MaxY - GridBounds.MinY);

    var boardWidth = bounds.Size.X + (borderMargin * 2);
    var boardHeight = bounds.Size.Y + (borderMargin * 2);
    var gameBoardAspectRatio = boardWidth / boardHeight;
    var viewportAspectRatio = GetViewport().GetVisibleRect().Size.X /
                              GetViewport().GetVisibleRect().Size.Y;

    var orthogonalSize = viewportAspectRatio > gameBoardAspectRatio
      ? boardHeight
      : boardWidth / viewportAspectRatio;


    var center = bounds.Position + (bounds.Size / 2);
    var height = orthogonalSize * 2;

    var newCameraPosition = new Vector3(center.X, height, center.Y);

    if (GridBounds.MaxX == 1 && GridBounds.MinX == -1 && GridBounds.MaxY == 1 && GridBounds.MinY == -1) {
      // Set, and don't animate, for the start position
      Camera.Size = orthogonalSize;
      Position = newCameraPosition;

      return;
    }

    const float duration = 0.3f;
    var tween = GetTree()
      .CreateTween()
      .SetParallel();
    tween
      .TweenProperty(Camera, "size", orthogonalSize, duration)
      .SetTrans(Tween.TransitionType.Sine)
      .SetEase(Tween.EaseType.InOut);
    tween
      .TweenProperty(this, "position", newCameraPosition, duration)
      .SetTrans(Tween.TransitionType.Sine)
      .SetEase(Tween.EaseType.InOut);
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
