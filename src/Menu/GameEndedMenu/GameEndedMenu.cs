namespace Vertex.Menu.Start;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IGameEndedMenu : IControl {
  event GameEndedMenu.RestartEventHandler Restart;
}

[Meta(typeof(IAutoNode))]
public partial class GameEndedMenu : Control, IGameEndedMenu {
  public override void _Notification(int what) => this.Notify(what);

  [Node]
  public Button RestartButton { get; set; } = default!;

  [Signal]
  public delegate void RestartEventHandler();

  public void OnReady() => RestartButton.Pressed += OnRestartButtonPressed;

  public void OnExitTree() => RestartButton.Pressed -= OnRestartButtonPressed;

  public void OnRestartButtonPressed() => EmitSignal(SignalName.Restart);
}
