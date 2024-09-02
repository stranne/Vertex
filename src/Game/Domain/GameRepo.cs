namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vertex.GridNode;

public interface IGameRepo : IDisposable {
  event Action<IGridNode[]>? AddNewGridNodes;

  void MouseEvent(IGridNode? gridNodeHovered, bool isLeftMouseButtonPressed);
  Color GetCurrentPlayerColor();
  Color GetPlayerColor(int playerId);
  void GridNodeSelected(IGridNode gridNode);
}

public class GameRepo(Color[] playerColors) : IGameRepo {
  private readonly Dictionary<Vector2I, IGridNode> _grid = [];

  private IGridNode? _gridNodeHovered;
  private int CurrentPlayerId { get; set; }

  public event Action<IGridNode[]>? AddNewGridNodes;

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

  public void GridNodeSelected(IGridNode gridNode) {
    var gridPosition = gridNode.GridPosition;
    _grid[gridPosition] = gridNode;
    var playerId = gridNode.SelectedByPlayerId
      ?? throw new InvalidOperationException("Grid node must be selected by a player.");

    var gameHasEnded = CheckWinningConditions(gridPosition, playerId);
    if (!gameHasEnded) {
      CurrentPlayerId = playerId++ % playerColors.Length;
    }
  }

  private bool CheckWinningConditions(Vector2I gridPosition, int playerId) {
    var gameHasEnded = false;

    Vector2I[] directions = [
      new(1, 0),
      new(0, 1),
      new(1, 1),
      new(1, -1),
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

  private void CreateNewGridNodes(Vector2I[] gridPositions) {
    var newGridNodes = gridPositions.Select(gridPosition => new GridNode {
      GridPosition = gridPosition
    }).ToList();

    newGridNodes.ForEach(gridNode => _grid[gridNode.GridPosition] = gridNode);
    AddNewGridNodes?.Invoke([.. newGridNodes]);
  }

  public void Dispose() => GC.SuppressFinalize(this);
}
