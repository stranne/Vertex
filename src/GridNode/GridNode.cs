namespace Vertex.GridNode;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Game.Domain;
using State;

public interface IGridNode : INode3D {
  Vector2I GridPosition { set; }

  void HoverEnter(Color color);
  void HoverExit();
  void Select();
  void InWinningLine(int lineIndex, int lineLength);
  void GameOver();
}

[Meta(typeof(IAutoNode))]
public partial class GridNode : Node3D, IGridNode {
  private const string ANIMATION_SPAWN_NAME = "spawn";
  private const string ANIMATION_HOVER_NAME = "hover";
  private const string ANIMATION_SELECT_NAME = "select";
  private const string ANIMATION_WINNING_LINE_NAME = "winning_line";

  public override void _Notification(int what) => this.Notify(what);

  public Vector2I GridPosition { get; set; } = default!;

  private Animation _animationHover = default!;
  private Animation _animationSelect = default!;
  private Animation _animationWinningLine = default!;
  private int _animationHoverAlbedoColorTrackIndex = default!;
  private int _animationSelectAlbedoColorTrackIndex = default!;
  private int _animationWinningLineAlbedoColorTrackIndex = default!;

  #region States
  public IGridNodeLogic GridNodeLogic { get; set; } = default!;
  public GridNodeLogic.IBinding GridNodeLogicBinding { get; set; } = default!;

  [Export]
  public Color DefaultColor { get; set; } = new(1, 1, 1, 1);

  [Export]
  public float DelayBetweenGridNodesInSeconds { get; set; } = 0.30f;

  [Export]
  public float DelayBetweenAnimationRestartInSeconds { get; set; } = 0.80f;

  #endregion

  #region Nodes
  [Node]
  public Node3D Pyramid { get; set; } = default!;

  [Node("%Pyramid/Cone")]
  public IMeshInstance3D PyramidMesh { get; set; } = default!;

  [Node]
  public ICollisionShape3D CollisionShape3D { get; set; } = default!;

  [Node]
  public IAnimationPlayer SelectionAnimationPlayer { get; set; } = default!;

  [Node]
  /// <remarks>Separate from <see cref="AnimationPlayer"/> to allow them to run in parallel.</remarks>
  public IAnimationPlayer SpawnAnimationPlayer { get; set; } = default!;

  [Node]
  public IAnimationPlayer WinningAnimationPlayer { get; set; } = default!;

  [Node]
  public ITimer WinningInitialDelayTimer { get; set; } = default!;

  [Node]
  public ITimer WinningLineDelayTimer { get; set; } = default!;
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

    _animationHover = SelectionAnimationPlayer.GetAnimation(ANIMATION_HOVER_NAME);
    _animationSelect = SelectionAnimationPlayer.GetAnimation(ANIMATION_SELECT_NAME);
    _animationWinningLine = WinningAnimationPlayer.GetAnimation(ANIMATION_WINNING_LINE_NAME);

    const string trackPath = "Pyramid/Cone:surface_material_override/0:albedo_color";
    _animationHoverAlbedoColorTrackIndex = _animationHover.GetAnimationTrackIndex(trackPath);
    _animationSelectAlbedoColorTrackIndex = _animationSelect.GetAnimationTrackIndex(trackPath);
    _animationWinningLineAlbedoColorTrackIndex = _animationWinningLine.GetAnimationTrackIndex(trackPath);

    // Ensure the first keyframe is same as material color
    _animationHover.TrackSetFirstKeyValue(_animationHoverAlbedoColorTrackIndex, DefaultColor);
  }

  public void OnResolved() {
    GridNodeMediator.Register(GridPosition, this);
    WinningInitialDelayTimer.Timeout += StartWinningLineAnimation;
    WinningLineDelayTimer.Timeout += StartWinningLineAnimation;

    GridNodeLogicBinding = GridNodeLogic.Bind();

    GridNodeLogicBinding
      .Handle((in GridNodeLogic.Output.Spawn _) => {
        CollisionShape3D.Disabled = false;
        SpawnAnimationPlayer.Play(ANIMATION_SPAWN_NAME);
        ((StandardMaterial3D)PyramidMesh.GetActiveMaterial(0)).AlbedoColor = DefaultColor;
      })
      .Handle((in GridNodeLogic.Output.Disabled _) =>
        CollisionShape3D.Disabled = true)
      .Handle((in GridNodeLogic.Output.HoverEntered output) => {
        SetHoverAnimationsColor(output.Color);
        SelectionAnimationPlayer.Play(ANIMATION_HOVER_NAME);
      })
      .Handle((in GridNodeLogic.Output.HoverExited _) =>
        SelectionAnimationPlayer.PlayBackwards(ANIMATION_HOVER_NAME))
      .Handle((in GridNodeLogic.Output.Selected output) =>
        ShowSelectedAnimation(output.Color))
      .Handle((in GridNodeLogic.Output.Winning output) => PrepareWinningLineAnimation(output.LineIndex, output.LineLength));

    GridNodeLogic.Start();
  }

  public void ExitTree() {
    WinningInitialDelayTimer.Timeout -= StartWinningLineAnimation;
    WinningLineDelayTimer.Timeout -= StartWinningLineAnimation;
    WinningInitialDelayTimer.Stop();
    WinningLineDelayTimer.Stop();
    GridNodeMediator.Unregister(GridPosition);
    GridNodeLogicBinding.Dispose();
  }

  public void HoverEnter(Color color) =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverEnter(color));

  public void HoverExit() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.HoverExit());

  public void Select() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Select());

  public void InWinningLine(int lineIndex, int lineLength) =>
    GridNodeLogic.Input(new GridNodeLogic.Input.InWinningLine(lineIndex, lineLength));

  public void GameOver() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.GameOver());

  private void SetHoverAnimationsColor(Color color) {
    // Set how much of the color should be applied on hover
    color.A = 0.4f;
    var mixedColor = DefaultColor.Blend(color);

    _animationHover.TrackSetLastKeyValue(_animationHoverAlbedoColorTrackIndex, mixedColor);
  }

  private void ShowSelectedAnimation(Color color) {
    SelectionAnimationPlayer.Play(ANIMATION_SELECT_NAME);
    // Get current color to pick up where any hovering animation might be
    var currentColor = GetCurrentMaterialColor();
    _animationHover.TrackSetFirstKeyValue(_animationHoverAlbedoColorTrackIndex, currentColor);
    _animationSelect.TrackSetLastKeyValue(_animationSelectAlbedoColorTrackIndex, color);
    _animationWinningLine.TrackSetFirstKeyValue(_animationWinningLineAlbedoColorTrackIndex, color);
  }

  private Color GetCurrentMaterialColor() =>
    ((StandardMaterial3D)PyramidMesh.GetSurfaceOverrideMaterial(0)).AlbedoColor;

  private static StandardMaterial3D CreateMaterial(Color color) =>
   new() {
     AlbedoColor = color
   };

  private void OnSpawnComplete() =>
    GridNodeLogic.Input(new GridNodeLogic.Input.Spawned());

  private void PrepareWinningLineAnimation(int lineIndex, int lineLength) {

    var animationLengthInSeconds = WinningAnimationPlayer.GetAnimation(ANIMATION_WINNING_LINE_NAME).Length;
    WinningLineDelayTimer.WaitTime = (animationLengthInSeconds * 2) + (DelayBetweenGridNodesInSeconds * lineLength) + DelayBetweenAnimationRestartInSeconds;

    if (lineIndex == 0) {
      StartWinningLineAnimation();
      return;
    }

    WinningInitialDelayTimer.WaitTime = DelayBetweenGridNodesInSeconds * lineIndex;
    WinningInitialDelayTimer.Start();
  }

  private void OnWinningLineAnimationReachedEnd() =>
    WinningAnimationPlayer.PlayBackwards(ANIMATION_WINNING_LINE_NAME);

  private void StartWinningLineAnimation() {
    WinningLineDelayTimer.Start();
    WinningAnimationPlayer.Play(ANIMATION_WINNING_LINE_NAME);
  }
}
