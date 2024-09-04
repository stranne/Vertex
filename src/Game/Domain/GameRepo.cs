namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IGameRepo : IDisposable {
  event Action? NewGame;
  event Action<Vector2I?, Color>? GridNodeHovered;
  event Action<Vector2I>? GridNodeClicked;
  event Action<List<Vector2I>>? PopulateGridPositions;
  event Action<List<Vector2I>>? GameEnded;

  void StartNewGame();
  void MouseEvent(Vector2I? hoveredGridPosition, bool isLeftMouseButtonPressed);
  void GridNodeSelected(Vector2I gridPosition);
}

public class GameRepo(Color[] playerColors) : IGameRepo {
  private const int NUMBER_IN_A_ROW_TO_WIN = 5;

  public event Action? NewGame;
  public event Action<Vector2I?, Color>? GridNodeHovered;
  public event Action<Vector2I>? GridNodeClicked;
  public event Action<List<Vector2I>>? PopulateGridPositions;
  public event Action<List<Vector2I>>? GameEnded;

  /// <remarks>
  /// <c>null</c> value indicates GridNode exists but isn't selected by any player.
  /// </remarks>
  private readonly Dictionary<Vector2I, int?> _grid = [];
  private Vector2I? _hoveredGridPosition;
  private int _currentPlayerId;

  public void StartNewGame() {
    _grid.Clear();
    _grid[Vector2I.Zero] = null;
    NewGame?.Invoke();
  }

  public void MouseEvent(Vector2I? hoveredGridPosition, bool isLeftMouseButtonPressed) {
    if (_hoveredGridPosition != hoveredGridPosition) {
      _hoveredGridPosition = hoveredGridPosition;
      GridNodeHovered?.Invoke(hoveredGridPosition, playerColors[_currentPlayerId]);
    }

    if (isLeftMouseButtonPressed && hoveredGridPosition.HasValue) {
      GridNodeClicked?.Invoke(hoveredGridPosition.Value);
    }
  }

  public void GridNodeSelected(Vector2I gridPosition) {
    _grid[gridPosition] = _currentPlayerId;

    var positionsInWinningLines = GetPositionsInWinningLines(gridPosition, _currentPlayerId);
    if (positionsInWinningLines.Count > 0) {
      GameEnded?.Invoke(positionsInWinningLines);
      return;
    }

    ChangeToNextPlayer();
    PopulateEmptyNeighborGridPositions(gridPosition);
  }

  private void ChangeToNextPlayer() =>
    _currentPlayerId = _currentPlayerId++ % playerColors.Length;

  private List<Vector2I> GetPositionsInWinningLines(Vector2I gridPosition, int playerId) {
    var gridPositionsInLines = new List<Vector2I>();

    Vector2I[] directions = [
      new(1, 0), // Horizontal
      new(0, 1), // Vertical
      new(1, 1), // Diagonal (top-left to bottom-right)
      new(1, -1), // Diagonal (bottom-left to top-right)
    ];

    foreach (var direction in directions) {
      var gridPositionsInLine = new List<Vector2I> { gridPosition };

      gridPositionsInLine.AddRange(CountInDirection(gridPosition, direction, playerId));
      gridPositionsInLine.AddRange(CountInDirection(gridPosition, -direction, playerId));

      if (gridPositionsInLine.Count >= NUMBER_IN_A_ROW_TO_WIN) {
        gridPositionsInLines.AddRange(gridPositionsInLine);
      }
    }

    return gridPositionsInLines;
  }

  private IEnumerable<Vector2I> CountInDirection(Vector2I startPosition, Vector2I direction, int playerId) {
    var currentPosition = startPosition + direction;

    while (_grid.TryGetValue(currentPosition, out var gridNodePlayer) && gridNodePlayer == playerId) {
      yield return currentPosition;
      currentPosition += direction;
    }
  }

  private void PopulateEmptyNeighborGridPositions(Vector2I gridPosition) {
    var emptyNeighborGridPositions = GetEmptyNeighborGridPositions(gridPosition).ToList();
    PopulateGridPositions?.Invoke(emptyNeighborGridPositions);
  }

  private IEnumerable<Vector2I> GetEmptyNeighborGridPositions(Vector2I gridPosition) {
    Vector2I[] neighborOffsets = [
      new (-1, -1),
      new (-1, 0),
      new (-1, 1),
      new (0, -1),
      new (0, 1),
      new (1, -1),
      new (1, 0),
      new (1, 1)
    ];

    foreach (var offset in neighborOffsets) {
      var neighborPosition = gridPosition + offset;

      if (!_grid.ContainsKey(neighborPosition)) {
        yield return neighborPosition;
      }
    }
  }

  public void Dispose() => GC.SuppressFinalize(this);
}
