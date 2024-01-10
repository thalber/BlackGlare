namespace BlackGlare;

/// <summary>
/// Visualizer that draws panels with messages for physicalobjects.
/// </summary>
public sealed class VisualizerRealEntityMessage : Visualizer<VisualizerRealEntityMessage>
{
	private Mod mod;
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
				if (!panels.TryGetValue(po, out PhysobjPanel panel) || !childNodes.Contains(panel))
				{
					string id = $"Panel_{po.GetHashCode()}";
					panel = new(id, this, po, mod.realEntityMessages);
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
	}
	public override void RoomChanged(Room? newRoom)
	{
		base.RoomChanged(newRoom);
	}
	private sealed class PhysobjPanel : AttachedPanel<PhysicalObject>
	{
		private Room room;
		public override string HeaderText => $"{item.GetType().Name} {item.abstractPhysicalObject.ID}";
		public PhysobjPanel(
			string id,
			VisualizerRealEntityMessage vis,
			PhysicalObject item,
			DescriptorSet<PhysicalObject> messages) : base(
				id,
				vis,
				item,
				messages)
		{
			room = item.room;
		}
		public override Vector2 GetAttachPos(RoomCamera cam, Vector2 camPos) => item.firstChunk.pos - camPos + UNSCRUNGLE_FUTILE;
		public override void Update(RoomCamera cam)
		{
			if (item.slatedForDeletetion
				|| item.abstractPhysicalObject.slatedForDeletion
				|| item.room != this.room)
			{
				slatedForDeletion = true;
			}
			base.Update(cam);
		}
	}
}
