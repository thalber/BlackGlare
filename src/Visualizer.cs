using System.Collections;
using System.Collections.Generic;

using System;

namespace BlackGlare;

public class Visualizer<TSelf> where TSelf : Visualizer<TSelf>, new()
{
	#region static state hooking
	private readonly static System.Runtime.CompilerServices.ConditionalWeakTable<RainWorldGame, TSelf> instances = new();
	private static BepInEx.Logging.ManualLogSource? logger;
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
		return new(typeof(TSelf), Undo);
	}
	public static void Undo()
	{
		On.RainWorldGame.ctor -= Hook_RWGConstructor;
		On.RainWorldGame.RawUpdate -= Hook_RWGRawUpdate;
		On.RainWorldGame.Update -= Hook_RWGUpdate;
	}
	private static void Hook_RWGConstructor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
	{
		try
		{
			orig(self, manager);
		}
		finally
		{
			instances.Add(self, Visualizer<TSelf>.Make(self));
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
				logger?.LogError($"Error on update");
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
	#endregion
	#region look guys im a normal class you can trust me (:
	public Room? room;
	public RainWorldGame? game;
	public readonly System.Collections.Generic.Dictionary<string, FNode> childNodes = new();
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

	}
	public virtual void AddNode(string key, FNode node)
	{
		Futile.stage.AddChild(node);
		childNodes.Add(key, node);
	}
	public virtual bool RemoveNode(string key)
	{
		switch (GetNode<FNode>(key))
		{
		case FNode node:
		{
			node.RemoveFromContainer();
			childNodes.Remove(key);
			return true;
		}
		default:
			return false;
		}
	}
	public virtual bool RemoveNode(FNode node)
	{
		if (childNodes.ContainsValue(node))
		{
			node.RemoveFromContainer();
			//todo: unfuck
			return childNodes.Remove(childNodes.Keys.First(k => childNodes[k] == node));
		}
		else
		{
			return false;
		}
	}
	public TNode? GetNode<TNode>(string key)
		where TNode : FNode
		=> childNodes.TryGetValue(key, out FNode node) ? node as TNode : null;
	public virtual void ClearNodes()
	{
		foreach ((string key, FNode node) in childNodes)
		{
			node.RemoveFromContainer();
		}
		childNodes.Clear();
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
}
