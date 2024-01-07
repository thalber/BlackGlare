namespace BlackGlare;

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
		for (int i = childNodes.Count - 1; i >= 0; i--)
		{
			PhysobjPanel panel = (PhysobjPanel)childNodes[i];
			RoomCamera? roomCamera = game?.cameras[0];
			if (roomCamera is null) continue;
			panel.Update(roomCamera, roomCamera.pos);
		}
	}
	public override void RoomChanged(Room? newRoom)
	{
		base.RoomChanged(newRoom);
	}
	internal sealed class PhysobjPanel : FContainer, IGetDestroyNotice<UpdatableAndDeletable>
	{
		private const float LINE_SPACING = 2f;
		private const float PADDING = 3f;
		private readonly float lineHeight;
		private readonly string id;
		private readonly VisualizerRealEntityMessage vis;
		private readonly PhysicalObject obj;
		private readonly MessageRegistry<PhysicalObject> messages;
		private readonly Room room;
		private readonly FSprite background;
		private readonly List<FLabel> messageLabels;
		private readonly FLabel header;
		//private readonly FLabel tag;
		private bool slatedForDeletion = false;

		public PhysobjPanel(
			string id,
			VisualizerRealEntityMessage vis,
			PhysicalObject obj,
			MessageRegistry<PhysicalObject> messages)
		{
			this.id = id;
			this.vis = vis;
			this.obj = obj;
			this.messages = messages;
			this.room = obj.room;
			if (!(vis.mod?.destroyNotifyReceivers.TryGetValue(obj, out _) ?? false))
			{
				vis.mod?.destroyNotifyReceivers.Add(obj, this);
			}
			lineHeight = Futile.atlasManager.GetFontWithName(GetFont()).lineHeight;
			header = new(GetFont(), $"{obj.GetType().Name} {obj.abstractPhysicalObject.ID}")
			{
				anchorX = 0f,
				anchorY = 1f,
				color = new(0.529f, 0.365f, 0.184f),
			};
			background = new FSprite("pixel")
			{
				anchorX = 0f,
				anchorY = 1f,
				color = new(0.3f, 0.3f, 0.3f),
				alpha = 0.5f
			};
			AddChild(header);
			AddChild(background);
			messageLabels = new();
		}
		public void Update(RoomCamera cam, Vector2 camPos)
		{
			Vector2
				origin = obj.firstChunk.pos - camPos + UNSCRUNGLE_FUTILE,
				bounds = Vector2.zero;
			bool drawAtAll = vis.isVisible && new Rect(Vector2.zero, cam.sSize).Contains(origin);
			float lh = (lineHeight + LINE_SPACING);
			void addedLine(float lw)
			{
				origin += new Vector2(0f, -lh);
				bounds.y += lh;
				bounds.x = Mathf.Max(bounds.x, lw);
			}
			background.SetPosition(origin - new Vector2(PADDING, PADDING));
			header.SetPosition(origin);
			addedLine(header.textRect.width);
			string[] requestedMessages = drawAtAll ? messages.GetAllMessages(obj).ToArray() : Array.Empty<string>();
			for (int i = 0; i < requestedMessages.Length || i < messageLabels.Count; i++)
			{
				bool drawMessage = requestedMessages.IndexInRange(i);
				FLabel currentLabel = GetOrAddLabel(i);
				currentLabel.MoveInFrontOfOtherNode(background);
				currentLabel.isVisible = drawMessage;
				if (!drawMessage)
				{
					continue;
				}
				currentLabel.text = requestedMessages[i];
				currentLabel.SetPosition(origin);
				float width = currentLabel.textRect.width;
				addedLine(width);
			}
			background.width = bounds.x + PADDING * 2f;
			background.height = bounds.y + PADDING * 2f;
			background.isVisible = header.isVisible = requestedMessages.Length is not 0;

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
		public void AddLabel(FLabel label)
		{
			AddChild(label);
			messageLabels.Add(label);
			//return messageLabels.Count - 1;
		}
		public FLabel GetOrAddLabel(int index)
		{
			if (messageLabels.IndexInRange(index))
			{
				return messageLabels[index];
			}
			else
			{
				AddLabel(new FLabel(GetFont(), "__TEXT__")
				{
					anchorX = 0f,
					anchorY = 1f
				});
				return messageLabels[messageLabels.Count - 1];
			}
		}
		void IGetDestroyNotice<UpdatableAndDeletable>.ObjectDestroyed(UpdatableAndDeletable thing)
		{
			slatedForDeletion = true;
			LogTrace($"Destroy notification received - {slatedForDeletion}");
		}
	}
}
