namespace Vertex.Game.Domain;

using System;
using Godot;
using Vertex.GridNode;

public interface IGameRepo : IDisposable {
  void Hover(IGridNode? gridNode);
  Color GetCurrentPlayerColor();
}

public class GameRepo : IGameRepo {
  private IGridNode? _gridNodeHovered;
  private int CurrentPlayerId { get; set; }

  public void Hover(IGridNode? gridNode) {
    if (_gridNodeHovered == gridNode) {
      return;
    }

    _gridNodeHovered?.OnHoverExit();
    _gridNodeHovered = gridNode;
    _gridNodeHovered?.OnHoverEnter();
  }

  public Color GetCurrentPlayerColor() => CurrentPlayerId switch {
    0 => new Color(1, 0, 0),
    1 => new Color(0, 1, 0),
    _ => throw new ArgumentOutOfRangeException(nameof(CurrentPlayerId), CurrentPlayerId, $"{nameof(CurrentPlayerId)} {CurrentPlayerId} isn't a valid number."),
  };

  public void Dispose() => GC.SuppressFinalize(this);
}
