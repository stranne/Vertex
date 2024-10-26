namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vertex.GridNode;

public interface IGridNodeMediator : IDisposable {
  event Action<Vector2I, IGridNode>? AddNewGridNode;

  void NewGame();
  Vector2I GetGridNodePosition(IGridNode gridNode);
  void SelectGridNode(Vector2I gridPosition);
  void PopulateGridPositions(List<Vector2I> gridPositions);
  void GameEnded(Dictionary<int, List<Vector2I>> inWinningLineGridPositions);

  void Register(Vector2I gridPosition, IGridNode gridNode);
  void Unregister(Vector2I gridPosition);
}

public class GridNodeMediator(PackedScene gridNodeScene) : IGridNodeMediator {
  private readonly Dictionary<Vector2I, IGridNode> _grid = [];

  public event Action<Vector2I, IGridNode>? AddNewGridNode;

  public void NewGame() {
    var centerGridNode = Vector2I.Zero;

    var gridsToRemove = _grid
      .Select(grid => grid.Key)
      .ToList();
    foreach (var gridToRemove in gridsToRemove) {
      _grid[gridToRemove].QueueFree();
      _grid.Remove(gridToRemove);
    }

    CreateNewGridNode(centerGridNode);
  }

  public Vector2I GetGridNodePosition(IGridNode gridNode) =>
    _grid.Single(grid => grid.Value == gridNode).Key;

  public void SelectGridNode(Vector2I gridPosition) =>
    _grid[gridPosition].Select();

  public void PopulateGridPositions(List<Vector2I> gridPositions) {
    foreach (var gridPosition in gridPositions) {
      CreateNewGridNode(gridPosition);
    }
  }

  public void GameEnded(Dictionary<int, List<Vector2I>> inWinningLineGridPositions) {
    foreach (var gridPosition in inWinningLineGridPositions) {
      foreach (var inWinningLineGridPosition in gridPosition.Value) {
        _grid[inWinningLineGridPosition].InWinningLine(gridPosition.Key, inWinningLineGridPositions.Count);
      }
    }

    var gameOverGridPositions = _grid.Keys.Except(inWinningLineGridPositions.Values.SelectMany(x => x)).ToList();
    foreach (var grid in gameOverGridPositions) {
      _grid[grid].GameOver();
    }
  }

  public void Register(Vector2I gridPosition, IGridNode gridNode) {
    if (_grid.ContainsKey(gridPosition)) {
      throw new ArgumentException($"Grid position {gridPosition} already exists.", nameof(gridPosition));
    }

    if (_grid.ContainsValue(gridNode)) {
      throw new ArgumentException($"GridNode {gridNode} already exists.", nameof(gridNode));
    }

    _grid[gridPosition] = gridNode;
  }

  public void Unregister(Vector2I gridPosition) {
    if (!_grid.ContainsKey(gridPosition)) {
      return;
    }

    _grid.Remove(gridPosition);
  }

  private void CreateNewGridNode(Vector2I gridPosition) {
    var newGridNode = gridNodeScene.Instantiate<GridNode>();
    newGridNode.Name = $"GridNode_{gridPosition}";
    newGridNode.GridPosition = gridPosition;
    AddNewGridNode?.Invoke(gridPosition, newGridNode);
  }

  public void Dispose() => GC.SuppressFinalize(this);
}
