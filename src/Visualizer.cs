namespace BlackGlare;

public class Visualizer<TSelf> where TSelf : Visualizer<TSelf>, new()
{
	#region static state hooking
	private readonly static System.Runtime.CompilerServices.ConditionalWeakTable<RainWorldGame, TSelf> instances = new();
	protected static BepInEx.Logging.ManualLogSource? logger;
	private static TSelf Make(RainWorldGame game)
	{
		TSelf creating = new();
		creating.Start(game);
		return creating;
	}
	public static VisualizerConcreteInfo Init(BepInEx.Logging.ManualLogSource? switchToLogger)
	{
		logger = switchToLogger;
		On.RainWorldGame.ctor += Hook_RWGConstructor;
		On.RainWorldGame.RawUpdate += Hook_RWGRawUpdate;
		On.RainWorldGame.Update += Hook_RWGUpdate;
		On.RainWorldGame.ShutDownProcess += Hook_RWGShutdown;
		return new(typeof(TSelf), Undo);
	}
	public static void Undo()
	{
		On.RainWorldGame.ctor -= Hook_RWGConstructor;
		On.RainWorldGame.RawUpdate -= Hook_RWGRawUpdate;
		On.RainWorldGame.Update -= Hook_RWGUpdate;
		On.RainWorldGame.ShutDownProcess -= Hook_RWGShutdown;
	}
	private static void Hook_RWGConstructor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
	{
		try
		{
			orig(self, manager);
		}
		finally
		{
			try
			{
				instances.Add(self, Visualizer<TSelf>.Make(self));
			}
			catch (Exception ex)
			{
				logger?.LogError($"Error on init");
				logger?.LogError(ex);
			}
		}
	}
	private static void Hook_RWGRawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float delta)
	{
		try
		{
			orig(self, delta);
		}
		finally
		{
			try
			{
				if (instances.TryGetValue(self, out TSelf instance))
				{
					instance.RawUpdate(delta);
				}
			}
			catch (Exception ex)
			{
				logger?.LogError($"Error on rawupdate");
				logger?.LogError(ex);
			}
		}
	}
	private static void Hook_RWGUpdate(On.RainWorldGame.orig_Update orig, RainWorldGame self)
	{
		try
		{
			orig(self);
		}
		finally
		{
			try
			{
				if (instances.TryGetValue(self, out TSelf instance))
				{
					instance.Update();
				}
			}
			catch (Exception ex)
			{
				logger?.LogError($"Error on update");
				logger?.LogError(ex);
			}
		}
	}
	private static void Hook_RWGShutdown(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
	{
		try
		{

			orig(self);
		}
		finally
		{
			try
			{
				if (instances.TryGetValue(self, out TSelf result))
				{
					result.ShutDown();
				}
			}
			catch (Exception ex)
			{
				logger?.LogError($"Error shutting down visualizer");
				logger?.LogError(ex);
			}
		}
	}
	#endregion
	#region look guys im a normal class you can trust me (:
	public bool isVisible = true;
	public Room? room;
	public RainWorldGame? game;
	public readonly List<FNode> childNodes = new();
	public readonly Dictionary<string, int> childNodeIndices = new();
	public virtual bool ClearSpritesOnRoomChange => true;
	public virtual void Start(RainWorldGame game)
	{
		this.game = game;
	}
	public virtual void RawUpdate(float delta)
	{
		if (game?.cameras[0].room != this.room)
		{
			RoomChanged(game?.cameras[0].room);
		}
	}
	public virtual void Update()
	{
		foreach (FNode node in childNodes)
		{
			node.isVisible = isVisible;
		}
	}
	public void ShutDown()
	{
		ClearNodes();
	}
	public virtual void AddNode(string key, FNode node)
	{
		Futile.stage.AddChild(node);
		childNodes.Add(node);
		childNodeIndices[key] = childNodes.Count - 1;
	}
	public virtual bool RemoveNode(string key)
	{
		switch (GetNode<FNode>(key))
		{
		case FNode node:
		{
			node.RemoveFromContainer();
			int index = childNodeIndices[key];
			childNodes.RemoveAt(index);
			childNodeIndices.Remove(key);
			(string Key, int Value)[]? needDecrement = childNodeIndices.Select(kvp => (kvp.Key, kvp.Value)).Where(pair => pair.Value > index).ToArray();
			foreach ((string candKey, int candIndex) in needDecrement)
			{
				childNodeIndices[candKey] = candIndex - 1;
			}
			return true;
		}
		default:
			return false;
		}
	}
	public virtual bool RemoveNode(FNode node)
	{
		if (childNodes.Contains(node))
		{
			node.RemoveFromContainer();
			//todo: unfuck
			int index = childNodes.IndexOf(node);
			childNodeIndices.Remove(childNodeIndices.Keys.FirstOrDefault(key => childNodeIndices[key] == index) ?? "");
			return childNodes.Remove(node);
		}
		else
		{
			return false;
		}
	}
	public TNode? GetNode<TNode>(string key)
		where TNode : FNode
	{
		if (childNodeIndices.TryGetValue(key, out int index)) return (TNode)childNodes[index];
		return null;
	}
	public virtual void ClearNodes()
	{
		foreach (FNode node in childNodes)
		{
			node.RemoveFromContainer();
		}
		childNodes.Clear();
		childNodeIndices.Clear();
	}
	public TNode GetOrCreateNode<TNode>(string key, Func<TNode> factory)
		where TNode : FNode
	{
		TNode? maybeResult = GetNode<TNode>(key);
		switch (maybeResult)
		{
		case TNode result:
			return result;
		default:
		{
			TNode newNode = factory();
			AddNode(key, newNode);
			return newNode;
		}
		}

	}
	public TNode GetOrCreateNode<TNode>(string key)
		where TNode : FNode, new()
		=> GetOrCreateNode<TNode>(key, () => new());

	public virtual void RoomChanged(Room? newRoom)
	{
		if (ClearSpritesOnRoomChange) ClearNodes();
		this.room = newRoom;
	}
	#endregion

	internal abstract class AttachedPanel<TItem> : FContainer
		where TItem : notnull
	{
		protected const float LINE_SPACING = 2f;
		protected const float PADDING = 3f;
		protected readonly float lineHeight;
		protected readonly string id;
		protected readonly Visualizer<TSelf> vis;
		public readonly TItem item;
		protected readonly DescriptorSet<TItem> messages;
		protected readonly FSprite background;
		protected readonly List<FLabel> messageLabels;
		protected readonly FLabel header;

		public virtual string HeaderText { get => item.ToString(); }
		public virtual Color HeaderCol { get => new(0.529f, 0.365f, 0.184f); }
		//protected abstract Vector2 AttachPos { get; }

		//private readonly FLabel tag;
		public bool slatedForDeletion = false;

		public AttachedPanel(
			string id,
			Visualizer<TSelf> vis,
			TItem item,
			DescriptorSet<TItem> messages)
		{
			this.id = id;
			this.vis = vis;
			this.item = item;
			this.messages = messages;
			//this.room = item.room;

			lineHeight = Futile.atlasManager.GetFontWithName(GetFont()).lineHeight;
			header = new(
				GetFont(),
				HeaderText
				//$"{item.GetType().Name} {item.abstractPhysicalObject.ID}"
				)
			{
				anchorX = 0f,
				anchorY = 1f,
				color = HeaderCol,
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
		public abstract Vector2 GetAttachPos(RoomCamera cam, Vector2 camPos);
		public virtual void Update(RoomCamera cam, Vector2 camPos)
		{
			Vector2
				origin = GetAttachPos(cam, camPos),//item.firstChunk.pos - camPos + UNSCRUNGLE_FUTILE,
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
			string[] requestedMessages = drawAtAll ? messages.GetAllMessages(item).ToArray() : Array.Empty<string>();
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

			if (
				this.slatedForDeletion
				// || item.slatedForDeletetion
				// || item.abstractPhysicalObject.slatedForDeletion
				// || item.room != room
				)
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
		// void IGetDestroyNotice<UpdatableAndDeletable>.ObjectDestroyed(UpdatableAndDeletable thing)
		// {
		// 	slatedForDeletion = true;
		// 	LogTrace($"Destroy notification received - {slatedForDeletion}");
		// }
	}
}
