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
    var bounds = new Rect2(minX, minY, maxX - minX, maxY - minY);
    var newPosition = CalculateNewPosition(bounds);
    _log.Print($"CameraRig: {newPosition}, minX: {minX}, maxX: {maxX}, minY: {minY}, maxY: {maxY}");

    Position = newPosition;
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

  private Vector3 CalculateNewPosition(Rect2 bounds) {
    const int borderMargin = 0;

    var center = bounds.Position + (bounds.Size / 2);
    var boardWidth = bounds.Size.X + borderMargin;
    var boardHeight = bounds.Size.Y + borderMargin;

    var aspectRatio = GetViewport().GetVisibleRect().Size.X / GetViewport().GetVisibleRect().Size.Y;

    float length;
    float fovRad;
    if (boardHeight > boardWidth) {
      length = boardHeight / 2;
      // Default field of view is vertical in Godot
      fovRad = Mathf.DegToRad(Camera.Fov);
    }
    else {
      length = boardWidth / 2;
      // Calculate horizontal field of view
      fovRad = 2.0f * Mathf.Atan(Mathf.Tan(Mathf.DegToRad(Camera.Fov) / 2.0f) * aspectRatio);
    }

    var yDistance = length / Mathf.Tan(fovRad / 2.0f);

    return new Vector3(center.X, yDistance, center.Y);
  }
}
