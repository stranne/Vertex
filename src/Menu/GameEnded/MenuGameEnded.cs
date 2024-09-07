namespace Vertex.Menu.Start;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IMenuGameEnded : IControl {
  event MenuGameEnded.RestartEventHandler Restart;
}

[Meta(typeof(IAutoNode))]
public partial class MenuGameEnded : Control, IMenuGameEnded {
  public override void _Notification(int what) => this.Notify(what);

  [Node]
  public Button RestartButton { get; set; } = default!;

  [Signal]
  public delegate void RestartEventHandler();

  public void OnReady() => RestartButton.Pressed += OnRestartButtonPressed;

  public void OnExitTree() => RestartButton.Pressed -= OnRestartButtonPressed;

  public void OnRestartButtonPressed() => EmitSignal(SignalName.Restart);
}
