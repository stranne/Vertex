namespace Vertex.GridNode;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using Vertex.GridNode.State;

[Meta(typeof(IAutoNode))]
public partial class GridNode : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  #region States
  public IGridNodeLogic GridNodeLogic { get; set; } = default!;
  public GridNodeLogic.IBinding GridNodeBinding { get; set; } = default!;
  #endregion

  #region Nodes
  [Node]
  public Node3D Pyramid { get; set; } = default!;

  [Node]
  public AnimationPlayer AnimationPlayer { get; set; } = default!;
  #endregion

  public void Setup() => GridNodeLogic = new GridNodeLogic();

  public void OnResolved() {
    GridNodeBinding = GridNodeLogic.Bind();

    GridNodeBinding
      .Handle((in GridNodeLogic.Output.SpawnStarted _) =>
        AnimationPlayer.Play("spawn")
      );

    GridNodeLogic.Start();
  }
}
