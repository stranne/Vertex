namespace Vertex.GridNode;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;
using Vertex.GridNode.State;

public interface IGridNode : INode3D {
  Vector2I GridPosition { set; }

  void Reset();
  void HoverEnter(Color color);
  void HoverExit();
  void Select();
  void InWinningLine();
  void GameOver();
}

[Meta(typeof(IAutoNode))]
public partial class GridNode : Node3D, IGridNode {
  private const string ANIMATION_HOVER_NAME = "hover";
  private const string ANIMATION_SELECT_NAME = "select";

  private Tween? _spawnTween;

  public override void _Notification(int what) => this.Notify(what);

  public Vector2I GridPosition { get; set; } = default!;

  private Animation _animationSpawn = default!;
  private Animation _animationHover = default!;
  private Animation _animationSelect = default!;
  private int _animationHoverTrackIndex = default!;
  private int _animationSelectTrackIndex = default!;

  #region States
  public IGridNodeLogic GridNodeLogic { get; set; } = default!;
  public GridNodeLogic.IBinding GridNodeLogicBinding { get; set; } = default!;

  [Export]
  public Color DefaultColor { get; set; } = new(1, 1, 1, 1);
  #endregion

  #region Nodes
  [Node]
  public Node3D Pyramid { get; set; } = default!;

  [Node("%Pyramid/Cone")]
  public IMeshInstance3D PyramidMesh { get; set; } = default!;

  [Node]
  public ICollisionShape3D CollisionShape3D { get; set; } = default!;

  [Node]
  public IAnimationPlayer AnimationPlayer { get; set; } = default!;
  #endregion

  #region Dependencies
  [Dependency]
  public IGameRepo GameRepo => this.DependOn<IGameRepo>();

  [Dependency]
  public IGridNodeMediator GridNodeMediator => this.DependOn<IGridNodeMediator>();
  #endregion

  public void Setup() {
    Position = new Vector3(GridPosition.X, 0, GridPosition.Y);

    GridNodeLogic = new GridNodeLogic();
    GridNodeLogic.Set(new GridNodeLogic.Data {
      GridPosition = GridPosition
    });

    var defaultMaterial = CreateMaterial(DefaultColor);
    PyramidMesh.SetSurfaceOverrideMaterial(0, defaultMaterial);

    _animationHover = AnimationPlayer.GetAnimation(ANIMATION_HOVER_NAME);
    _animationSelect = AnimationPlayer.GetAnimation(ANIMATION_SELECT_NAME);
    const string trackPath = "Pyramid/Cone:surface_material_override/0:albedo_color";
    _animationHoverTrackIndex = GetAnimationTrackIndex(_animationHover, trackPath);
    _animationSelectTrackIndex = GetAnimationTrackIndex(_animationSelect, trackPath);

    // Ensure the first keyframe is same as material color
    SetAnimationFirsTrackKeyColor(_animationHover, _animationHoverTrackIndex, DefaultColor);
  }

  public void OnResolved() {
    GridNodeMediator.Register(GridPosition, this);

    GridNodeLogicBinding = GridNodeLogic.Bind();

    GridNodeLogicBinding
      .Handle((in GridNodeLogic.Output.Spawn _) => {
        CollisionShape3D.Disabled = false;
        AnimateSpawn();
      })
      .Handle((in GridNodeLogic.Output.Disabled _) =>
        CollisionShape3D.Disabled = true
      )
      .Handle((in GridNodeLogic.Output.HoverEntered output) => {
        SetHoverAnimationsColor(output.Color);
        AnimationPlayer.Play(ANIMATION_HOVER_NAME);
      })
      .Handle((in GridNodeLogic.Output.HoverExited _) =>
        AnimationPlayer.PlayBackwards(ANIMATION_HOVER_NAME))
      .Handle((in GridNodeLogic.Output.Selected output) =>
        ShowSelectedAnimation(output.Color)
      );

    GridNodeLogic.Start();
  }

  public void ExitTree() {
    GridNodeMediator.Unregister(GridPosition);

    if (_spawnTween != null) {
      // Use if instead of null conditional to avoid CS0079 warning
      _spawnTween.Finished -= OnSpawnComplete;
    }

    GridNodeLogicBinding.Dispose();
  }

  public void Reset() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Reset());

  public void HoverEnter(Color color) =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverEnter(color));

  public void HoverExit() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverExit());

  public void Select() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Select());

  public void InWinningLine() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.InWinningLine());

  public void GameOver() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.GameOver());

  private void SetHoverAnimationsColor(Color color) {
    // Set how much of the color should be applied on hover
    color.A = 0.4f;
    var mixedColor = DefaultColor.Blend(color);

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
    SetAnimationFirsTrackKeyColor(_animationHover, _animationHoverTrackIndex, DefaultColor);
    SetAnimationLastTrackKeyColor(_animationSelect, _animationSelectTrackIndex, color);
  }

  private static void SetAnimationFirsTrackKeyColor(Animation animation, int trackIndex, Color color) =>
    animation.TrackSetKeyValue(trackIndex, 0, color);

  private static void SetAnimationLastTrackKeyColor(Animation animation, int trackIndex, Color color) {
    var lastTrackKey = animation.TrackGetKeyCount(trackIndex) - 1;
    animation.TrackSetKeyValue(trackIndex, lastTrackKey, color);
  }

  private Color GetCurrentMaterialColor() =>
    ((StandardMaterial3D)PyramidMesh.GetSurfaceOverrideMaterial(0)).AlbedoColor;

  private static StandardMaterial3D CreateMaterial(Color color) =>
   new() {
     AlbedoColor = color
   };

  private void AnimateSpawn() {
    var data = new List<TweenPropertyData<float>>() {
      new(0.0f, 0.0f),
      new(0.85f, 0.3f),
      new(0.65f, 0.2f),
      new(0.75f, 0.2f),
    };

    _spawnTween = GetTree().CreateTween();
    foreach (var tweenPropertyData in data) {
      var scaleVector = new Vector3(tweenPropertyData.Data, tweenPropertyData.Data, tweenPropertyData.Data);
      _spawnTween.TweenProperty(Pyramid, "scale", scaleVector, tweenPropertyData.Duration)
        .SetTrans(Tween.TransitionType.Cubic)
        .SetEase(Tween.EaseType.Out);
    }

    _spawnTween.Finished += OnSpawnComplete;
  }

  private void OnSpawnComplete() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Spawned());

  private sealed record TweenPropertyData<T>(T Data, float Duration);
}
