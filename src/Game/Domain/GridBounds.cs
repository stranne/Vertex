namespace Vertex.Game.Domain;

using Godot;

public interface IGridBounds {
  event GridBounds.Bounds BoundsUpdated;

  void UpdateBounds(Vector2I gridPosition);
  void Reset();
}

public class GridBounds : IGridBounds {
  public delegate void Bounds(int minX, int maxX, int minY, int maxY);
  public event Bounds? BoundsUpdated;

  private int _minX;
  private int _maxX;
  private int _minY;
  private int _maxY;

  public void UpdateBounds(Vector2I gridPosition) {
    var boundsUpdated = false;

    if (gridPosition.X - 1 <= _minX) {
      _minX = gridPosition.X - 1;
      boundsUpdated = true;
    }
    if (gridPosition.X + 1 >= _maxX) {
      _maxX = gridPosition.X + 1;
      boundsUpdated = true;
    }
    if (gridPosition.Y - 1 <= _minY) {
      _minY = gridPosition.Y - 1;
      boundsUpdated = true;
    }
    if (gridPosition.Y + 1 >= _maxY) {
      _maxY = gridPosition.Y + 1;
      boundsUpdated = true;
    }

    if (boundsUpdated) {
      NotifyListeners();
    }
  }

  public void Reset() {
    _minX = 1;
    _maxX = 1;
    _minY = 1;
    _maxY = 1;

    NotifyListeners();
  }

  private void NotifyListeners() =>
    BoundsUpdated?.Invoke(_minX, _maxX, _minY, _maxY);
}
