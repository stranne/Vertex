namespace Vertex.GridNode;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode.State;

public interface IGridNode : INode3D {
  Vector2I GridPosition { get; }
}

[Meta(typeof(IAutoNode))]
public partial class GridNode : Node3D, IGridNode {
  private const string ANIMATION_HOVER_NAME = "hover";
  private const string ANIMATION_SELECT_NAME = "select";

  public override void _Notification(int what) => this.Notify(what);

  private Color _defaultMaterialColor = default!;
  private Animation _animationHover = default!;
  private Animation _animationSelect = default!;
  private int _animationHoverTrackIndex = default!;
  private int _animationSelectTrackIndex = default!;

  public required Vector2I GridPosition { get; init; }

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
    Position = new Vector3(GridPosition.X, 0, GridPosition.Y);

    GridNodeLogic = new GridNodeLogic();
    GridNodeLogic.Set(new GridNodeLogic.Data {
      GridPosition = GridPosition
    });

    _defaultMaterialColor = GetCurrentMaterialColor();
    _animationHover = AnimationPlayer.GetAnimation(ANIMATION_HOVER_NAME);
    _animationSelect = AnimationPlayer.GetAnimation(ANIMATION_SELECT_NAME);
    _animationHoverTrackIndex = GetAnimationTrackIndex(_animationHover, "Pyramid:surface_material_override/0:albedo_color");
    _animationSelectTrackIndex = GetAnimationTrackIndex(_animationSelect, "Pyramid:surface_material_override/0:albedo_color");
    // Ensure the first keyframe is same as material color
    SetAnimationFirsTrackKeyColor(_animationHover, _animationHoverTrackIndex, _defaultMaterialColor);
  }

  public void OnResolved() {
    GridNodeLogic.Set(GameRepo);

    GridNodeLogicBinding = GridNodeLogic.Bind();

    SpawnAnimated += OnSpawnAnimated;

    GridNodeLogicBinding
      .Handle((in GridNodeLogic.Output.SpawnStarted _) => {
        CollisionShape.Disabled = true;
        AnimationPlayer.Play("spawn");
      })
      .Handle((in GridNodeLogic.Output.SpawnComplete _) =>
        CollisionShape.Disabled = false
      )
      .Handle((in GridNodeLogic.Output.Disabled _) =>
        CollisionShape.Disabled = true
      )
      .Handle((in GridNodeLogic.Output.HoverEntered output) => {
        SetHoverAnimationsColor(output.Color);
        AnimationPlayer.Play(ANIMATION_HOVER_NAME);
      })
      .Handle((in GridNodeLogic.Output.HoverExited _) =>
        AnimationPlayer.PlayBackwards(ANIMATION_HOVER_NAME)
      )
      .Handle((in GridNodeLogic.Output.Selected output) => {
        ShowSelectedAnimation(output.Color);
        GameRepo.GridNodeSelected(GridPosition);
      });

    GridNodeLogic.Start();
  }

  public void ExitTree() {
    SpawnAnimated -= OnSpawnAnimated;

    GridNodeLogicBinding.Dispose();
  }

  public void OnSpawnAnimated() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Spawned());

  public void OnHoverEnter(Color color) =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverEnter() {
      Color = color
    });

  public void OnHoverExit() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverExit());

  public void OnClicked(int currentPlayerId) =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Select());

  public void OnGameOver() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.GameOver());

  public void OnInWinningLine() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.InWinningLine());

  private void SetHoverAnimationsColor(Color color) {
    // Set how much of the color should be applied on hover
    color.A = 0.4f;
    var mixedColor = _defaultMaterialColor.Blend(color);

    var lastTrackKey = _animationHover.TrackGetKeyCount(_animationHoverTrackIndex) - 1;
    _animationHover.TrackSetKeyValue(_animationHoverTrackIndex, lastTrackKey, mixedColor);
  }

  private static int GetAnimationTrackIndex(Animation animation, string trackPath) {
    var trackIndex = animation.FindTrack(trackPath, Animation.TrackType.Value);
    return trackIndex == -1
      ? throw new InvalidOperationException($"{trackPath} track not found.")
      : trackIndex;
  }

  private void ShowSelectedAnimation(Color color) {
    AnimationPlayer.Play(ANIMATION_SELECT_NAME);
    // Get current color to pick up where any hovering animation might be
    var currentColor = GetCurrentMaterialColor();
    SetAnimationFirsTrackKeyColor(_animationHover, _animationHoverTrackIndex, _defaultMaterialColor);
    SetAnimationLastTrackKeyColor(_animationSelect, _animationSelectTrackIndex, color);
  }

  private static void SetAnimationFirsTrackKeyColor(Animation animation, int trackIndex, Color color) =>
    animation.TrackSetKeyValue(trackIndex, 0, color);

  private static void SetAnimationLastTrackKeyColor(Animation animation, int trackIndex, Color color) {
    var lastTrackKey = animation.TrackGetKeyCount(trackIndex) - 1;
    animation.TrackSetKeyValue(trackIndex, lastTrackKey, color);
  }

  private Color GetCurrentMaterialColor() =>
    ((StandardMaterial3D)Pyramid.GetSurfaceOverrideMaterial(0)).AlbedoColor;
}
