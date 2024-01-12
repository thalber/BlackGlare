namespace BlackGlare;

/// <summary>
/// Auto-attaches to RWG. CRTP. Significant static state applies hooks.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public abstract class Visualizer<TSelf> where TSelf : Visualizer<TSelf>, new()
{
	#region static state hooking
	protected readonly static System.Runtime.CompilerServices.ConditionalWeakTable<RainWorldGame, TSelf> instances = new();
	protected static BepInEx.Logging.ManualLogSource? logger;
	private static bool enabled = false;
	protected static TSelf Make(RainWorldGame game)
	{
		TSelf creating = new();
		creating.Start(game);
		return creating;
	}
	/// <summary>
	/// Applies hooks.
	/// </summary>
	/// <param name="switchToLogger">Optional bepinex logsource to report errors to. </param>
	/// <returns>Stuff necessary to undo hooks and clear statics without knowing a concrete type.</returns>
	public static VisualizerConcreteInfo Init(BepInEx.Logging.ManualLogSource? switchToLogger)
	{
		if (enabled) throw new InvalidOperationException($"{typeof(TSelf)} is already enabled");
		logger = switchToLogger;
		Mod mod = UnityEngine.Object.FindObjectOfType<Mod>();
		mod.OnRWGCreate += OnCreate;
		mod.OnRWGRawUpdate += OnRawUpdate;
		mod.OnRWGUpdate += OnUpdate;
		mod.OnRWGShutdown += OnShutdown;
		enabled = true;
		return new(typeof(TSelf), Undo);
	}
	/// <summary>
	/// Undoes hooks.
	/// </summary>
	public static void Undo()
	{
		Mod mod = UnityEngine.Object.FindObjectOfType<Mod>();
		mod.OnRWGCreate -= OnCreate;
		mod.OnRWGRawUpdate -= OnRawUpdate;
		mod.OnRWGUpdate -= OnUpdate;
		mod.OnRWGShutdown -= OnShutdown;
		enabled = false;
	}
	public static void OnCreate(RainWorldGame self)
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
	public static void OnRawUpdate(RainWorldGame self, float delta)
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
	public static void OnUpdate(RainWorldGame self)
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
	public static void OnShutdown(RainWorldGame self)
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
	#endregion
	#region look guys im a normal class you can trust me (:
	/// <summary>
	/// Whether visualizer should be drawn.
	/// </summary>
	public bool isVisible { get; protected set; } = true;
	/// <summary>
	/// Keeps track of current room.
	/// </summary>
	public Room? room { get; protected set; }
	public RainWorldGame? game { get; protected set; }
	public Mod mod;
	/// <summary>
	/// All fnodes in this visualizer. Modifying directly is not advised.
	/// </summary>
	protected readonly List<FNode> childNodes = new();
	/// <summary>
	/// Links keys to <see cref="childNodes"/> indices. Modifying directly is not advised.
	/// </summary>
	/// <returns></returns>
	protected readonly Dictionary<string, int> childNodeIndices = new();
	/// <summary>
	/// Contains all nodes that should receive update calls.
	/// </summary>
	/// <returns></returns>
	protected readonly List<IVisDynamicNode> dynamicNodes = new();
	/// <summary>
	/// Indicates whether the visualizer wants to clear all sprites on switching rooms.
	/// </summary>
	public virtual bool ClearSpritesOnRoomChange => true;
	/// <summary>
	/// Since we can't have proper constructor constraints on generics, this is unfortunately the ""constructor"". Called on RWG constructor.
	/// </summary>
	public virtual void Start(RainWorldGame game)
	{
		this.game = game;
		mod = UnityEngine.Object.FindObjectOfType<Mod>();
	}
	/// <summary>
	/// Bound to RWG.RawUpdate.
	/// </summary>
	/// <param name="delta"></param>
	public virtual void RawUpdate(float delta)
	{
		if (game?.cameras[0].room != this.room)
		{
			RoomChanged(game?.cameras[0].room);
		}
	}
	/// <summary>
	/// Bound to RWG.Update.
	/// </summary>
	public virtual void Update()
	{
		RoomCamera roomCamera = game.cameras[0];
		foreach (FNode node in childNodes)
		{
			node.isVisible = isVisible;
		}
		for (int i = dynamicNodes.Count - 1; i >= 0; i--)
		{
			IVisDynamicNode nodeAsLC = dynamicNodes[i];
			FNode node = (FNode)nodeAsLC;
			try
			{
				nodeAsLC.Update(roomCamera);
				if (nodeAsLC.slatedForDeletion)
				{
					RemoveNode(node);
				}
			}
			catch (Exception ex)
			{
				logger?.LogError($"error on update for object {nodeAsLC.ToString()}");
				logger?.LogError(ex);
			}
		}

	}
	/// <summary>
	/// Bound to RWG.ShutDownProcess.
	/// </summary>
	public void ShutDown()
	{
		ClearNodes();
	}
	protected void ToggleVisible()
	{
		isVisible = !isVisible;
	}
	protected T? FindOtherVis<T>()
		where T : Visualizer<T>, new()
	{
		if (this.game is null) return null;
		Visualizer<T>.instances.TryGetValue(this.game, out T? result);
		return result;
	}
	/// <summary>
	/// Adds a new child node to child list and futile scene.
	/// </summary>
	/// <param name="key">Node's ID (has to be unique).</param>
	/// <param name="node">FNode itself.</param>
	public virtual void AddNode(string key, FNode node)
	{
		if (childNodeIndices.ContainsKey(key))
		{
			throw new ArgumentException($"There is already a child node with key {key}. Node keys have to be unique.");
		}
		Futile.stage.AddChild(node);
		childNodes.Add(node);
		if (node is IVisDynamicNode dynNode) dynamicNodes.Add(dynNode);
		childNodeIndices[key] = childNodes.Count - 1;

	}
	/// <summary>
	/// Removes a node under specified key.
	/// </summary>
	/// <param name="key">Node's key.</param>
	/// <returns>Whether removal was successful.</returns>
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
			if (node is IVisDynamicNode dynNode) dynamicNodes.Remove(dynNode);
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
	/// <summary>
	/// Removes a specified node.
	/// </summary>
	/// <param name="node">Node itself.</param>
	/// <returns>Whether removal was successful.</returns>
	public virtual bool RemoveNode(FNode node)
	{
		if (childNodes.Contains(node))
		{
			node.RemoveFromContainer();
			//todo: unfuck
			int index = childNodes.IndexOf(node);
			return RemoveNode(childNodeIndices.Keys.FirstOrDefault(key => childNodeIndices[key] == index) ?? "");
		}
		else
		{
			return false;
		}
	}
	/// <summary>
	/// Attempts to get a node by specified key.
	/// </summary>
	public TNode? GetNode<TNode>(string key)
		where TNode : FNode
	{
		if (childNodeIndices.TryGetValue(key, out int index)) return (TNode)childNodes[index];
		return null;
	}
	/// <summary>
	/// Removes all nodes from the stage and clears node list.
	/// </summary>
	public virtual void ClearNodes()
	{
		foreach (FNode node in childNodes)
		{
			node.RemoveFromContainer();
		}
		childNodes.Clear();
		childNodeIndices.Clear();
		dynamicNodes.Clear();
	}
	/// <summary>
	/// Gets a node by specified key, creates a new one if needed.
	/// </summary>
	/// <param name="key">Node's ID.</param>
	/// <param name="makeNode">TNode creator function.</param>
	/// <typeparam name="TNode">Node's type.</typeparam>
	/// <returns>Node obtained or newly created.</returns>
	public TNode GetOrCreateNode<TNode>(string key, Func<TNode> makeNode)
		where TNode : FNode
	{
		TNode? maybeResult = GetNode<TNode>(key);
		switch (maybeResult)
		{
		case TNode result:
			return result;
		default:
		{
			TNode newNode = makeNode();
			AddNode(key, newNode);
			return newNode;
		}
		}
	}
	/// <summary>
	/// Gets a node by specified key, creates a new one if needed. Node type has to have a parameterless constructor.
	/// </summary>
	/// <param name="key">Node's ID.</param>
	/// <typeparam name="TNode">Node's type.</typeparam>
	/// <returns>Node obtained or newly created.</returns>
	public TNode GetOrCreateNode<TNode>(string key)
		where TNode : FNode, new()
		=> GetOrCreateNode<TNode>(key, () => new());
	/// <summary>
	/// Called when camera switches between rooms.
	/// </summary>
	public virtual void RoomChanged(Room? newRoom)
	{
		if (ClearSpritesOnRoomChange) ClearNodes();
		this.room = newRoom;
	}
	#endregion
	/// <summary>
	/// Panel that tracks something in the game and uses a descriptorset to display labels for it.
	/// </summary>
	/// <typeparam name="TItem">Type of tracked item.</typeparam>
	protected abstract class AttachedPanel<TItem> : FContainer, IVisDynamicNode
		where TItem : notnull
	{
		protected float lineSpacing = 2f;
		protected float padding = 3f;
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
		public bool slatedForDeletion { get; set; } = false;
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
		/// <summary>
		/// Gets the desired upper left point of the panel.
		/// </summary>
		public abstract Vector2 GetAttachPos(RoomCamera cam, Vector2 camPos);
		public virtual void Update(RoomCamera cam)
		{
			Vector2
				camPos = cam.pos,
				origin = GetAttachPos(cam, camPos),//item.firstChunk.pos - camPos + UNSCRUNGLE_FUTILE,
				bounds = Vector2.zero;
			bool drawAtAll = vis.isVisible && new Rect(Vector2.zero, cam.sSize).Contains(origin);
			float lh = (lineHeight + lineSpacing);
			void addedLine(float lw)
			{
				origin += new Vector2(0f, -lh);
				bounds.y += lh;
				bounds.x = Mathf.Max(bounds.x, lw);
			}
			background.SetPosition(origin - new Vector2(padding, padding));
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
			background.width = bounds.x + padding * 2f;
			background.height = bounds.y + padding * 2f;
			background.isVisible = header.isVisible = requestedMessages.Length is not 0;

			// if (
			// 	this.slatedForDeletion
			// 	)
			// {
			// 	try
			// 	{
			// 		bool success = vis.RemoveNode(this);
			// 		LogTrace($"Destroying a panel success : {success}");
			// 	}
			// 	catch (Exception ex)
			// 	{
			// 		LogError(ex);
			// 	}
			// }
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
	}
}
