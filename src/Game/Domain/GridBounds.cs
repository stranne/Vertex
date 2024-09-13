namespace Vertex.Game.Domain;

using System;
using Godot;

public interface IGridBounds {
  event Action BoundsUpdated;

  int MinX { get; }
  int MaxX { get; }
  int MinY { get; }
  int MaxY { get; }

  void UpdateBounds(Vector2I gridPosition);
  void Reset();
}

public class GridBounds : IGridBounds {
  public event Action? BoundsUpdated;

  public int MinX { get; private set; } = -1;
  public int MaxX { get; private set; } = -1;
  public int MinY { get; private set; } = -1;
  public int MaxY { get; private set; } = -1;

  public void UpdateBounds(Vector2I gridPosition) {
    var boundsUpdated = false;

    if (gridPosition.X - 1 <= MinX) {
      MinX = gridPosition.X - 1;
      boundsUpdated = true;
    }
    if (gridPosition.X + 1 >= MaxX) {
      MaxX = gridPosition.X + 1;
      boundsUpdated = true;
    }
    if (gridPosition.Y - 1 <= MinY) {
      MinY = gridPosition.Y - 1;
      boundsUpdated = true;
    }
    if (gridPosition.Y + 1 >= MaxY) {
      MaxY = gridPosition.Y + 1;
      boundsUpdated = true;
    }

    if (boundsUpdated) {
      BoundsUpdated?.Invoke();
    }
  }

  public void Reset() {
    MinX = -1;
    MaxX = 1;
    MinY = -1;
    MaxY = 1;

    BoundsUpdated?.Invoke();
  }
}
