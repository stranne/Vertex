namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GoDotLog;
using Godot;
using Vertex.GridNode;

public interface IGameRepo : IDisposable {
  void StartNewGame();
  void MouseEvent(IGridNode? hoveredGridNode, bool isLeftMouseButtonPressed);
  void AddGridNode(Vector2I gridPosition);
  Color GetCurrentPlayerColor();
}

public class GameRepo(Color[] playerColors, IGridNodeMediator gridNodeMediator) : IGameRepo {
  private const int NUMBER_IN_A_ROW_TO_WIN = 5;

  private ILog _log = new GDLog(nameof(GameRepo));


  /// <remarks>
  /// <c>null</c> value indicates GridNode exists but isn't selected by any player.
  /// </remarks>
  private readonly Dictionary<Vector2I, int?> _grid = [];
  private IGridNode? _hoveredGridNode;
  private int _currentPlayerId;

  public void StartNewGame() {
    if (_grid.Count != 0) {
      _grid.Clear();
      _grid[Vector2I.Zero] = null;
    }

    gridNodeMediator.NewGame();
  }

  public void MouseEvent(IGridNode? hoveredGridNode, bool isLeftMouseButtonPressed) {
    if (_hoveredGridNode != hoveredGridNode) {
#if DEBUG
      var previous = _hoveredGridNode == null
        ? "null"
        : gridNodeMediator.GetGridNodePosition(_hoveredGridNode).ToString();
      var next = hoveredGridNode == null
        ? "null"
        : gridNodeMediator.GetGridNodePosition(hoveredGridNode).ToString();
      _log.Print($"Hovered GridNode: last {previous}, new {next}");
#endif
      _hoveredGridNode?.HoverExit();
      _hoveredGridNode = hoveredGridNode;
      _hoveredGridNode?.HoverEnter(GetCurrentPlayerColor());
    }

    if (isLeftMouseButtonPressed && hoveredGridNode != null) {
      var gridPosition = gridNodeMediator.GetGridNodePosition(hoveredGridNode);
      if (_grid[gridPosition] != null) {
        // Grid position already selected, ignoring click.
        return;
      }

      GridNodeSelected(gridPosition);
    }
  }

  public void AddGridNode(Vector2I gridPosition) {
    if (_grid.ContainsKey(gridPosition)) {
      throw new InvalidOperationException($"GridNode already exists at {gridPosition}");
    }

    _grid[gridPosition] = null;
  }

  private void GridNodeSelected(Vector2I gridPosition) {
    _grid[gridPosition] = _currentPlayerId;
    gridNodeMediator.SelectGridNode(gridPosition);

    var positionsInWinningLines = GetPositionsInWinningLines(gridPosition, _currentPlayerId);
    if (positionsInWinningLines.Count > 0) {
      gridNodeMediator.GameEnded(positionsInWinningLines);
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
    gridNodeMediator.PopulateGridPositions(emptyNeighborGridPositions);
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

  public Color GetCurrentPlayerColor() => playerColors[_currentPlayerId];

  public void Dispose() => GC.SuppressFinalize(this);
}
