namespace BlackGlare;

[BepInEx.BepInPlugin(ID, DISPLAYNAME, VERSION)]
public class Mod : BepInEx.BaseUnityPlugin
{
	public const string ID = "thalber.blackglare";
	public const string DISPLAYNAME = "Black Glare";
	public const string VERSION = "0.1";
	internal DescriptorSet<AbstractWorldEntity> abstractEntityMessages = new();
	internal DescriptorSet<PhysicalObject> realEntityMessages = new();
	internal DescriptorSet<AbstractRoom> roomMessages = new();

	internal System.Runtime.CompilerServices.ConditionalWeakTable<UpdatableAndDeletable, IGetDestroyNotice<UpdatableAndDeletable>> destroyNotifyReceivers = new();

	public void OnEnable()
	{
		__SwitchToBepinexLogger(Logger);
		__writeCallsiteInfo = false;
		__writeTrace = true;
		VisualizerRealEntityMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + " Get Real"));
		VisualizerRoomMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + " Roomies"));
		On.UpdatableAndDeletable.Destroy += Hook_NotifyUADDestroy;

		Selector<PhysicalObject>.Downcast<Player> selector = new();
		// Descriptor<PhysicalObject>.ByCallback providerRollCounter = new (selector, Guid.NewGuid(), (po) => $"roll counter {((Player)po).rollCounter}");
		// Descriptor<PhysicalObject>.ByCallback providerJumpBoost = new (selector, Guid.NewGuid(), (po) => $"jump boost {((Player)po).jumpBoost}");
		// realEntityMessages.AddDescriptor(providerRollCounter);
		// realEntityMessages.AddDescriptor(providerJumpBoost);

		// Selector<AbstractRoom>.ByCallback selector2 = new((room) => true);
		// Descriptor<AbstractRoom>.ByCallback numberOfEntities = new(selector2, Guid.NewGuid(), (room) => $"Number of entitites {room.entities.Count}");
		// roomMessages.AddDescriptor(numberOfEntities);

		//Func<Player, bool> 
		API.Labels.AddObjectLabel<Player>(player => $"jmp {player.jumpBoost}");
		API.Labels.AddObjectLabel<Player>(player => $"rll {player.rollCounter}");
		API.Labels.AddObjectLabel<Fly>(fly => fly.flap.ToString())
			.AddCondition(fly => !fly.dead);
		API.Labels.AddRoomLabel(room => $"ec {room.entities.Count}");
	}
	public void Hook_NotifyUADDestroy(On.UpdatableAndDeletable.orig_Destroy orig, UpdatableAndDeletable self)
	{
		orig(self);
		if (destroyNotifyReceivers.TryGetValue(self, out IGetDestroyNotice<UpdatableAndDeletable> ign)) ign.ObjectDestroyed(self);
	}
}


