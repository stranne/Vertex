namespace Vertex.InGameAudio;

public partial class InGameAudioLogic {
  public static class Output {
    public readonly record struct NewGame;
    public readonly record struct GameEnded;
    public readonly record struct GridNodeSelected;
  }
}
