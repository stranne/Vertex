namespace Vertex.Menu.GameEnded;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IStartMenu : IControl {
  event StartMenu.StartGameEventHandler StartGame;
}

[Meta(typeof(IAutoNode))]
public partial class StartMenu : Control, IStartMenu {
  public override void _Notification(int what) => this.Notify(what);

  [Node]
  public Button NewGameButton { get; set; } = default!;

  [Signal]
  public delegate void StartGameEventHandler();

  public void OnReady() => NewGameButton.Pressed += OnStartGameButtonPressed;

  public void OnExitTree() => NewGameButton.Pressed -= OnStartGameButtonPressed;

  public void OnStartGameButtonPressed() => EmitSignal(SignalName.StartGame);
}
