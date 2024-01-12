namespace BlackGlare;

public sealed class VisualizerRoomMessage : Visualizer<VisualizerRoomMessage>
{
	//private Mod mod;
	private DescriptorSet<AbstractRoom>.WrapFrom<DevInterface.RoomPanel> descriptorSetWrapper;
	private readonly Dictionary<DevInterface.RoomPanel, roomExtrasPanel> panels = new();
	public override void Start(RainWorldGame game)
	{
		base.Start(game);
		descriptorSetWrapper = new(mod.roomMessages, (panel) => panel.roomRep.room);
	}
	public override void Update()
	{
		base.Update();
		if (game?.devUI is DevInterface.DevUI devtools
			&& devtools.activePage is DevInterface.MapPage mapPage)
		{
			for (int i = 0; i < mapPage.subNodes.Count; i++)
			{
				DevInterface.DevUINode node = mapPage.subNodes[i];
				if (node is not DevInterface.RoomPanel vanillaPanel) continue;
				try
				{
					if (!panels.TryGetValue(vanillaPanel, out roomExtrasPanel? extrasPanel) || !childNodes.Contains(extrasPanel))
					{
						string id = $"{vanillaPanel.roomRep.room.name}";
						extrasPanel = new(id, this, vanillaPanel, descriptorSetWrapper);
						AddNode(id, extrasPanel);
						panels[vanillaPanel] = extrasPanel;
					}
				}
				catch (Exception ex)
				{
					logger?.LogError($"error on create panel for room {vanillaPanel.roomRep.room.name}");
					logger?.LogError(ex);
				}
			}
		}
	}
	private sealed class roomExtrasPanel : AttachedPanel<DevInterface.RoomPanel>
	{
		private DevInterface.DevUI? devUI => vis.game?.devUI;
		public override string HeaderText => "";
		public roomExtrasPanel(
			string id,
			Visualizer<VisualizerRoomMessage> vis,
			DevInterface.RoomPanel item,
			DescriptorSet<DevInterface.RoomPanel> messages) : base(
				id,
				vis,
				item,
				messages)
		{
		}
		public override void Update(RoomCamera cam)
		{
			if (devUI is null || devUI.activePage is not DevInterface.MapPage mapPage)
			{
				slatedForDeletion = true;
			}
			base.Update(cam);
		}
		public override Vector2 GetAttachPos(RoomCamera cam, Vector2 camPos) => item.absPos + item.size;
	}

}
