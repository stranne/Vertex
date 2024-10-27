namespace Vertex.InGameAudio;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public interface IInGameAudioLogic : ILogicBlock<InGameAudioLogic.State>;

[Meta]
[LogicBlock(typeof(State))]
public partial class InGameAudioLogic :
  LogicBlock<InGameAudioLogic.State>, IInGameAudioLogic {
  public override Transition GetInitialState() => To<State>();
}
