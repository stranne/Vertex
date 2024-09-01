namespace Vertex.GridNode;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode.State;

public interface IGridNode : INode3D {
  void OnHoverEnter();
  void OnHoverExit();
}

[Meta(typeof(IAutoNode))]
public partial class GridNode : Node3D, IGridNode {
  private const string ANIMATION_HOVER_NAME = "hover";

  public override void _Notification(int what) => this.Notify(what);

  private Color _pyramidMaterialColor = default!;
  private Animation _animationHover = default!;
  private int _animationPlayerHoverTrackIndex = default!;

  #region Signals
  [Signal]
  public delegate void SpawnAnimatedEventHandler();
  #endregion

  #region States
  public IGridNodeLogic GridNodeLogic { get; set; } = default!;
  public GridNodeLogic.IBinding GridNodeLogicBinding { get; set; } = default!;
  #endregion

  #region Nodes
  [Node]
  public IMeshInstance3D Pyramid { get; set; } = default!;

  /// <remarks>
  /// Is disabled by default to block hovering and select until after spawning completes.
  /// </remarks>
  [Node]
  public ICollisionShape3D CollisionShape { get; set; } = default!;

  [Node]
  public IAnimationPlayer AnimationPlayer { get; set; } = default!;
  #endregion

  #region Dependencies
  [Dependency]
  public IGameRepo GameRepo => this.DependOn<IGameRepo>();
  #endregion

  public void Setup() {
    GridNodeLogic = new GridNodeLogic();

    _pyramidMaterialColor = ((StandardMaterial3D)Pyramid.GetSurfaceOverrideMaterial(0)).AlbedoColor;
    _animationPlayerHoverTrackIndex = GetAnimationPlayerHoverTrackIndex();
    // Ensure the first keyframe is the original color
    _animationHover.TrackSetKeyValue(_animationPlayerHoverTrackIndex, 0, _pyramidMaterialColor);
  }

  public void OnResolved() {
    GridNodeLogicBinding = GridNodeLogic.Bind();

    SpawnAnimated += OnSpawnAnimated;

    GridNodeLogicBinding
      .Handle((in GridNodeLogic.Output.SpawnStarted _) =>
        AnimationPlayer.Play("spawn")
      )
      .Handle((in GridNodeLogic.Output.SpawnComplete _) =>
        CollisionShape.Disabled = false
      )
      .Handle((in GridNodeLogic.Output.HoverEntered _) => {
        SetHoverAnimationsPlayerColor();
        AnimationPlayer.Play(ANIMATION_HOVER_NAME);
      })
      .Handle((in GridNodeLogic.Output.HoverExited _) =>
        AnimationPlayer.PlayBackwards(ANIMATION_HOVER_NAME)
      );

    GridNodeLogic.Start();
  }

  public void ExitTree() {
    SpawnAnimated -= OnSpawnAnimated;

    GridNodeLogicBinding.Dispose();
  }

  public void OnSpawnAnimated() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Spawned());

  public void OnHoverEnter() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverEnter());

  public void OnHoverExit() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverExit());

  private void SetHoverAnimationsPlayerColor() {

    var playerColor = GameRepo.GetCurrentPlayerColor();
    // Set how much of the player color should be applied on hover
    playerColor.A = 0.4f;
    var color = _pyramidMaterialColor.Blend(playerColor);

    var lastTrackKey = _animationHover.TrackGetKeyCount(_animationPlayerHoverTrackIndex) - 1;
    _animationHover.TrackSetKeyValue(_animationPlayerHoverTrackIndex, lastTrackKey, color);
  }

  private int GetAnimationPlayerHoverTrackIndex() {
    const string trackPath = "Pyramid:surface_material_override/0:albedo_color";

    _animationHover = AnimationPlayer.GetAnimation(ANIMATION_HOVER_NAME);
    var trackIndex = _animationHover.FindTrack(trackPath, Animation.TrackType.Value);

    return trackIndex == -1
      ? throw new InvalidOperationException($"{trackPath} track not found.")
      : trackIndex;
  }
}
