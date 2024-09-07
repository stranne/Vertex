namespace Vertex.Menu.GameEnded;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IMenuStart : IControl {
  event MenuStart.StartGameEventHandler StartGame;
}

[Meta(typeof(IAutoNode))]
public partial class MenuStart : Control, IMenuStart {
  public override void _Notification(int what) => this.Notify(what);

  [Node]
  public Button NewGameButton { get; set; } = default!;

  [Signal]
  public delegate void StartGameEventHandler();

  public void OnReady() => NewGameButton.Pressed += OnStartGameButtonPressed;

  public void OnExitTree() => NewGameButton.Pressed -= OnStartGameButtonPressed;

  public void OnStartGameButtonPressed() => EmitSignal(SignalName.StartGame);
}
