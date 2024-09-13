namespace Vertex.GridBoard;

using System.Buffers;
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

  public void OnResolved() =>
    GridBounds.BoundsUpdated += OnBoundsUpdated;

  public void ExitTree() =>
    GridBounds.BoundsUpdated -= OnBoundsUpdated;

  public void OnBoundsUpdated(int minX, int maxX, int minY, int maxY) {
    const float marginFactor = 0.1f;

    var width = (maxX - minX) * (1 + marginFactor);
    var height = (maxY - minY) * (1 + marginFactor);

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

    var centerX = (minX + maxX) / 2.0f;
    var centerZ = (minY + maxY) / 2.0f;
    Position = new Vector3(centerX, 0, centerZ);

    Scale = new Vector3(visibleWidth, 1, visibleHeight);
  }
}
