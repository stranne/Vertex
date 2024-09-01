namespace Vertex.Game.Domain;

using System;
using Godot;
using Vertex.GridNode;

public interface IGameRepo : IDisposable {
  void MouseEvent(IGridNode? gridNodeHovered, bool isLeftMouseButtonPressed);
  Color GetCurrentPlayerColor();
  Color GetPlayerColor(int playerId);
}

public class GameRepo : IGameRepo {
  private IGridNode? _gridNodeHovered;
  private int CurrentPlayerId { get; set; }

  public void MouseEvent(IGridNode? gridNodeHovered, bool isLeftMouseButtonPressed) {
    if (isLeftMouseButtonPressed) {
      _gridNodeHovered?.OnClicked(CurrentPlayerId);
    }

    if (_gridNodeHovered == gridNodeHovered) {
      return;
    }

    _gridNodeHovered?.OnHoverExit();
    _gridNodeHovered = gridNodeHovered;
    _gridNodeHovered?.OnHoverEnter();
  }

  public Color GetCurrentPlayerColor() => GetPlayerColor(CurrentPlayerId);

  public Color GetPlayerColor(int playerId) => playerId switch {
    0 => new Color(1, 0, 0),
    1 => new Color(0, 1, 0),
    _ => throw new ArgumentOutOfRangeException(nameof(playerId), playerId, $"{nameof(playerId)} {playerId} isn't a valid number."),
  };

  public void Dispose() => GC.SuppressFinalize(this);
}
