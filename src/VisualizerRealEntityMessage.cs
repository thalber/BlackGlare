namespace BlackGlare;

public sealed class VisualizerRealEntityMessage : Visualizer<VisualizerRealEntityMessage>
{
	private Mod? mod;
	private System.Runtime.CompilerServices.ConditionalWeakTable<PhysicalObject, Extras> extraPOStuff = new();
	private readonly Dictionary<PhysicalObject, PhysobjPanel> panels = new();
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
			try
			{

				if (uad is not PhysicalObject po) continue;
				if (!panels.TryGetValue(po, out PhysobjPanel panel))
				{
					string id = $"Panel_{po.GetHashCode()}";
					panel = new(id, this, po);
					AddNode(id, panel);
					panels[po] = panel;
				}
			}
			catch (Exception ex)
			{
				LogError("Error on create panel");
				LogError(ex);
			}
		}
		for (int i = childNodes.Count - 1; i >= 0; i--)
		{
			PhysobjPanel panel = (PhysobjPanel)childNodes[i];
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
	internal sealed class PhysobjPanel : FContainer, IGetDestroyNotice<UpdatableAndDeletable>
	{
		private readonly string id;
		private readonly VisualizerRealEntityMessage vis;
		private readonly PhysicalObject obj;
		//private readonly MessageRegistry<PhysicalObject> registry;
		private readonly Room room;
		private readonly FLabel tag;
		private bool slatedForDeletion = false;

		public PhysobjPanel(string id, VisualizerRealEntityMessage vis, PhysicalObject obj /*, MessageRegistry<PhysicalObject> registry*/)
		{
			this.id = id;
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
			vis.mod?.destroyNotifyReceivers.Add(obj, this);
		}
		public void Update(Vector2 camPos)
		{
			tag.SetPosition(obj.firstChunk.pos - camPos);
			tag.text = slatedForDeletion ? "dead" : "alive";
			if (this.slatedForDeletion || obj.slatedForDeletetion || obj.room != room)
			{
				try
				{
					bool success = vis.RemoveNode(this);
					LogTrace($"Destroying a panel success : {success}");
				}
				catch (Exception ex)
				{
					LogError(ex);
				}
			}
		}

		void IGetDestroyNotice<UpdatableAndDeletable>.ObjectDestroyed(UpdatableAndDeletable thing)
		{
			slatedForDeletion = true;
			LogTrace($"Destroy notification received - {slatedForDeletion}");
		}
	}
}
