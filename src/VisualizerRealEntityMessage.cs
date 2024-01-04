namespace BlackGlare;

public sealed class VisualizerRealEntityMessage : Visualizer<VisualizerRealEntityMessage>
{
	private Mod? mod;
	private System.Runtime.CompilerServices.ConditionalWeakTable<PhysicalObject, Extras> extraPOStuff = new();
	private readonly System.Runtime.CompilerServices.ConditionalWeakTable<PhysicalObject, PhysobjPanel> panels = new();
	public override void Start(RainWorldGame game)
	{
		base.Start(game);
		mod = UnityEngine.Object.FindObjectOfType<Mod>();
	}
	public override void Update()
	{
		base.Update();
		if (room is null) return;
		foreach (UpdatableAndDeletable uad in room.updateList)
		{
			if (uad is not PhysicalObject po) continue;
			PhysobjPanel panel = panels.GetValue(po, (phys) => new(this, phys));
			if (!childNodes.ContainsValue(panel))
			{
				AddNode($"Panel_{po.GetHashCode()}", panel);
			}
			panel.Update(game?.cameras[0].pos ?? Vector2.zero);
		}
	}
	public override void RoomChanged(Room? newRoom)
	{
		base.RoomChanged(newRoom);
	}

	private class Extras
	{
		public int numberOfLabels = -1;
	}
	internal sealed class PhysobjPanel : FContainer
	{
		private readonly VisualizerRealEntityMessage vis;
		private readonly PhysicalObject obj;
		//private readonly MessageRegistry<PhysicalObject> registry;
		private readonly Room room;
		private readonly FLabel tag;

		public PhysobjPanel(VisualizerRealEntityMessage vis, PhysicalObject obj /*, MessageRegistry<PhysicalObject> registry*/)
		{
			this.vis = vis;
			this.obj = obj;
			//this.registry = registry;
			this.room = obj.room;
			tag = new(GetFont(), obj.GetType().FullName)
			{
				anchorX = 0f,
				anchorY = 0f,
			};
			this.AddChild(tag);
		}
		public void Update(Vector2 camPos)
		{
			tag.SetPosition(obj.firstChunk.pos - camPos);
			if (obj.room != room)
			{
				vis.RemoveNode(this);
			}
		}
	}
}
