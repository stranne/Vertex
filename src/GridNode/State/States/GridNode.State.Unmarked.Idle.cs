namespace Vertex.GridNode.State;

using Chickensoft.Introspection;
using Godot;
using Vertex.Game.Domain;

public partial class GridNodeLogic {
  public abstract partial record State {
    [Meta]
    public partial record Idle : Unmarked, IGet<Input.HoverEnter> {
      public Idle() {
        OnAttach(() => {
          var gameRepo = Get<IGameRepo>();
          gameRepo.GridNodeHovered += OnGridNodeHovered;
        });
        OnDetach(() => {
          var gameRepo = Get<IGameRepo>();
          gameRepo.GridNodeHovered -= OnGridNodeHovered;
        });
      }

      public Transition On(in Input.HoverEnter input) => To<Hover>();

      public void OnGridNodeHovered(Vector2I? gridPosition, Color color) {
        if (Data.GridPosition == gridPosition) {
          return;
        }

        Data.Color = color;
        Output(new Output.HoverEntered(color));
      }
    }
  }
}
