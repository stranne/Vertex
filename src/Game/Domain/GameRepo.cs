namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vertex.GridNode;

public interface IGameRepo : IDisposable {
  event Action<List<Vector2I>>? PopulateGridPositions;

  void MouseEvent(IGridNode? gridNodeHovered, bool isLeftMouseButtonPressed);
  Color GetCurrentPlayerColor();
  Color GetPlayerColor(int playerId);
  void GridNodeSelected(IGridNode gridNode);
  void Reset();
}

public class GameRepo(Color[] playerColors) : IGameRepo {
  // TODO remove and replace with signals?
  private readonly Dictionary<Vector2I, IGridNode> _grid = [];

  private IGridNode? _gridNodeHovered;
  private int CurrentPlayerId { get; set; }

  public event Action<List<Vector2I>>? PopulateGridPositions;

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

  public Color GetPlayerColor(int playerId) => playerColors[playerId];

  public void Reset() {
    // TODO Remove existing grid nodes, and ensure single one in center.
    var gridNodesToRemove = _grid.Where(grid => grid.Key != Vector2I.Zero);
    foreach (var (gridPosition, gridNode) in gridNodesToRemove) {
      gridNode.QueueFree();
      _grid.Remove(gridPosition);
    }
  }

  public void GridNodeSelected(IGridNode gridNode) {
    var gridPosition = gridNode.GridPosition;
    _grid[gridPosition] = gridNode;
    var playerId = gridNode.SelectedByPlayerId
      ?? throw new InvalidOperationException("Grid node must be selected by a player.");

    var gameHasEnded = CheckWinningConditions(gridPosition, playerId);
    if (!gameHasEnded) {
      ChangeToNextPlayer(playerId);
      var emptyNeighborGridPositions = GetEmptyNeighborGridPositions(gridPosition).ToList();
      PopulateGridPositions?.Invoke(emptyNeighborGridPositions);
    }
  }

  private void ChangeToNextPlayer(int playerId) =>
    CurrentPlayerId = playerId++ % playerColors.Length;

  private bool CheckWinningConditions(Vector2I gridPosition, int playerId) {
    var gameHasEnded = false;

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

      if (gridPositionsInLine.Count >= 5) {
        gameHasEnded = true;
        TriggerWinning(gridPositionsInLine);
      }
    }

    if (gameHasEnded) {
      _grid.Values.ToList().ForEach(gridNode => gridNode.OnGameOver());
    }

    return gameHasEnded;
  }

  private IEnumerable<Vector2I> CountInDirection(Vector2I startPosition, Vector2I direction, int playerId) {
    var currentPosition = startPosition + direction;

    while (_grid.TryGetValue(currentPosition, out var gridNode) && gridNode.SelectedByPlayerId == playerId) {
      yield return currentPosition;
      currentPosition += direction;
    }
  }

  private void TriggerWinning(IList<Vector2I> gridPositions) =>
    gridPositions.Select(gridPosition => _grid[gridPosition])
      .ToList()
      .ForEach(gridNode => gridNode.OnInWinningLine());

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
