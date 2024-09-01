namespace Vertex.Test;

using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using Chickensoft.GodotTestDriver;
using Vertex.Game;

public class GameTest(Node testScene) : TestClass(testScene) {
  private Game _sut = default!;
  private Fixture _fixture = default!;

  [SetupAll]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _sut = await _fixture.LoadAndAddScene<Game>();
  }

  [CleanupAll]
  public void Cleanup() => _fixture.Cleanup();
}
