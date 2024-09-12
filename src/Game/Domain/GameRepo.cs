namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GoDotLog;
using Godot;
using Vertex.GridNode;

public interface IGameRepo : IDisposable {
  event Action NewGame;
  event Action GameEnded;
  event GameRepo.GridNode GridNodeSelected;

  void StartNewGame();
  void MouseEvent(IGridNode? hoveredGridNode, bool isLeftMouseButtonPressed);
  void AddGridNode(Vector2I gridPosition);
  Color GetCurrentPlayerColor();
}

public class GameRepo(Color[] playerColors, IGridNodeMediator gridNodeMediator) : IGameRepo {
  private const int NUMBER_IN_A_ROW_TO_WIN = 5;

  private readonly GDLog _log = new(nameof(GameRepo));

  /// <remarks>
  /// <c>null</c> value indicates GridNode exists but isn't selected by any player.
  /// </remarks>
  private readonly Dictionary<Vector2I, int?> _grid = [];
  private readonly IGridBounds _gridBounds = new GridBounds();
  private IGridNode? _hoveredGridNode;
  private int _currentPlayerId;

  public event Action? NewGame;
  public event Action? GameEnded;

  public delegate void GridNode(Vector2I gridPosition);
  public event GridNode? GridNodeSelected;

  public void StartNewGame() {
    if (_grid.Count != 0) {
      _grid.Clear();
      _grid[Vector2I.Zero] = null;
    }

    _hoveredGridNode = null;
    gridNodeMediator.NewGame();
    NewGame?.Invoke();
  }

  public void MouseEvent(IGridNode? hoveredGridNode, bool isLeftMouseButtonPressed) {
    if (_hoveredGridNode != hoveredGridNode) {
      // _log.Print($"Hovering: {_hoveredGridNode?.Name ?? "null"} -> {hoveredGridNode?.Name ?? "null"}");
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

      // _log.Print($"Selecting: {hoveredGridNode.Name}");
      ProcessGridNodeSelection(gridPosition);
    }
  }

  public void AddGridNode(Vector2I gridPosition) {
    if (_grid.ContainsKey(gridPosition)) {
      throw new InvalidOperationException($"GridNode already exists at {gridPosition}");
    }

    _grid[gridPosition] = null;
  }

  private void ProcessGridNodeSelection(Vector2I gridPosition) {
    _grid[gridPosition] = _currentPlayerId;
    gridNodeMediator.SelectGridNode(gridPosition);
    GridNodeSelected?.Invoke(gridPosition);

    var positionsInWinningLines = GetPositionsInWinningLines(gridPosition, _currentPlayerId);
    if (positionsInWinningLines.Count > 0) {
      _log.Print($"Player {_currentPlayerId} won with {positionsInWinningLines.Count} in a row. Positions: {string.Join(", ", positionsInWinningLines)}");
      gridNodeMediator.GameEnded(positionsInWinningLines);
      GameEnded?.Invoke();
      return;
    }

    ChangeToNextPlayer();
    PopulateEmptyNeighborGridPositions(gridPosition);
  }

  private void ChangeToNextPlayer() =>
    _currentPlayerId = (_currentPlayerId + 1) % playerColors.Length;

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
