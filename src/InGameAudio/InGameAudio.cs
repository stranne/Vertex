namespace Vertex.InGameAudio;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;


public interface IInGameAudio : INode { }

[Meta(typeof(IAutoNode))]
public partial class InGameAudio : Node, IInGameAudio {
  public override void _Notification(int what) => this.Notify(what);

  #region Nodes
  [Node]
  public IAudioStreamPlayer NewGame { get; set; } = default!;
  [Node]
  public IAudioStreamPlayer GameEnded { get; set; } = default!;
  [Node]
  public IAudioStreamPlayer GridNodeSelected { get; set; } = default!;
  #endregion Nodes

  #region Dependencies
  [Dependency]
  public IGameRepo GameRepo => this.DependOn<IGameRepo>();
  #endregion Dependencies

  #region State
  public IInGameAudioLogic InGameAudioLogic { get; set; } = default!;

  public InGameAudioLogic.IBinding InGameAudioBinding { get; set; } = default!;
  #endregion State

  public void Setup() =>
    InGameAudioLogic = new InGameAudioLogic();

  public void OnResolved() {
    InGameAudioLogic.Set(GameRepo);

    InGameAudioBinding = InGameAudioLogic.Bind();

    InGameAudioBinding
      .Handle((in InGameAudioLogic.Output.NewGame _) =>
        NewGame.Play())
      .Handle((in InGameAudioLogic.Output.GameEnded _) =>
        GameEnded.Play())
      .Handle((in InGameAudioLogic.Output.GridNodeSelected _) =>
        GridNodeSelected.Play());

    InGameAudioLogic.Start();
  }

  public void OnExitTree() {
    InGameAudioLogic.Stop();
    InGameAudioBinding.Dispose();
  }
}
