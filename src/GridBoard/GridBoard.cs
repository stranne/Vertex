namespace Vertex.GridBoard;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;

public interface IGridBoard : INode3D { }

[Meta(typeof(IAutoNode))]
public partial class GridBoard : Node3D, IGridBoard {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public IGridBounds GridBounds => this.DependOn<IGridBounds>();

  public void OnResolved() {
    GridBounds.BoundsUpdated += UpdateScaleAndPosition;
    GetViewport().SizeChanged += UpdateScaleAndPosition;
  }

  public void ExitTree() {
    GridBounds.BoundsUpdated -= UpdateScaleAndPosition;
    GetViewport().SizeChanged -= UpdateScaleAndPosition;
  }

  public void UpdateScaleAndPosition() {
    const float marginFactor = 0.5f;

    var width = (GridBounds.MaxX - GridBounds.MinX) * (1 + marginFactor);
    var height = (GridBounds.MaxY - GridBounds.MinY) * (1 + marginFactor);

    var boardAspectRatio = width / height;
    var viewportAspectRatio = GetViewport().GetVisibleRect().Size.X /
                              GetViewport().GetVisibleRect().Size.Y;

    float visibleWidth, visibleHeight;
    if (viewportAspectRatio > boardAspectRatio) {
      visibleHeight = height;
      visibleWidth = visibleHeight * viewportAspectRatio;
    }
    else {
      visibleWidth = width;
      visibleHeight = visibleWidth / viewportAspectRatio;
    }

    var centerX = (GridBounds.MinX + GridBounds.MaxX) / 2.0f;
    var centerZ = (GridBounds.MinY + GridBounds.MaxY) / 2.0f;

    Scale = new Vector3(visibleWidth, 1, visibleHeight);
    Position = new Vector3(centerX, 0, centerZ);
  }
}
