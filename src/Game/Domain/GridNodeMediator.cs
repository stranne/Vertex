namespace Vertex.Game.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GodotNodeInterfaces;
using Godot;
using Vertex.GridNode;

public interface IGridNodeMediatorForGameRepo {
  event Action<INode3D>? AddNewGridNode;

  public void NewGame();
  public void MouseEvent(IGridNode? hoveredGridNode, Color color, bool isLeftMouseButtonPressed);
  public void PopulateGridPositions(List<Vector2I> gridPositions);
  public void GameEnded(List<Vector2I> inWinningLineGridPositions);
}

public interface IGridNodeMediatorForGridNode {
  void Register(Vector2I gridPosition, IGridNode gridNode);
  void Unregister(Vector2I gridPosition);
}

public interface IGridNodeMediator : IGridNodeMediatorForGameRepo, IGridNodeMediatorForGridNode, IDisposable { }

public class GridNodeMediator(PackedScene gridNodeScene) : IGridNodeMediator {
  private readonly Dictionary<Vector2I, IGridNode> _grid = [];

  private IGridNode? _hoveredGridNode;

  public event Action<INode3D>? AddNewGridNode;

  public void NewGame() {
    var centerGridNode = Vector2I.Zero;

    if (_grid.Count == 0) {
      CreateNewGridNode(centerGridNode);
      return;
    }

    var gridsToRemove = _grid
      .Where(grid => grid.Key != centerGridNode)
      .Select(grid => grid.Key)
      .ToList();
    foreach (var gridToRemove in gridsToRemove) {
      _grid[gridToRemove].QueueFree();
      _grid.Remove(gridToRemove);
    }

    _grid[centerGridNode].Reset();
  }

  public void MouseEvent(IGridNode? hoveredGridNode, Color color, bool isLeftMouseButtonPressed) {
    if (_hoveredGridNode != hoveredGridNode) {
      _hoveredGridNode?.HoverExit();
      _hoveredGridNode = hoveredGridNode;
      _hoveredGridNode?.HoverEnter(color);
    }

    if (isLeftMouseButtonPressed && hoveredGridNode != null) {
      hoveredGridNode.Clicked();
    }
  }

  public void PopulateGridPositions(List<Vector2I> gridPositions) {
    foreach (var gridPosition in gridPositions) {
      CreateNewGridNode(gridPosition);
    }
  }

  public void GameEnded(List<Vector2I> inWinningLineGridPositions) {
    foreach (var gridPosition in inWinningLineGridPositions) {
      _grid[gridPosition].InWinningLine();
    }

    var gameOverGridPositions = _grid.Keys.Except(inWinningLineGridPositions).ToList();
    foreach (var grid in gameOverGridPositions) {
      _grid[grid].GameOver();
    }
  }

  public void Register(Vector2I gridPosition, IGridNode gridNode) {
    if (_grid.ContainsKey(gridPosition)) {
      throw new ArgumentException($"Grid position {gridPosition} already exists.", nameof(gridPosition));
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
    var newGridNode = gridNodeScene.Instantiate<IGridNode>();
    newGridNode.GridPosition = gridPosition;
    AddNewGridNode?.Invoke(newGridNode);
  }

  public void Dispose() => GC.SuppressFinalize(this);
}
