using System;
using System.Collections;
using System.Collections.Generic;

namespace BlackGlare;

[BepInEx.BepInPlugin("thalber.blackglare", "Black Glare", "0.1")]
public class Mod : BepInEx.BaseUnityPlugin
{
	//internal BepInEx.Configuration.ConfigEntry<bool> enableUI;
	internal DescriptorSet<AbstractWorldEntity> abstractEntityMessages = new();
	internal DescriptorSet<PhysicalObject> realEntityMessages = new();
	internal DescriptorSet<AbstractRoom> roomMessages = new();

	internal System.Runtime.CompilerServices.ConditionalWeakTable<UpdatableAndDeletable, IGetDestroyNotice<UpdatableAndDeletable>> destroyNotifyReceivers = new();

	public void OnEnable()
	{
		
		__SwitchToBepinexLogger(Logger);
		__writeCallsiteInfo = false;
		__writeTrace = true;
		VisualizerRealEntityMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + "GetReal"));
		On.UpdatableAndDeletable.Destroy += Hook_NotifyUADDestroy;

		Selector<PhysicalObject>.Downcast<Player> selector = new();
		Descriptor<PhysicalObject>.ByCallback providerRollCounter = new (selector, Guid.NewGuid(), (po) => $"roll counter {((Player)po).rollCounter}");
		Descriptor<PhysicalObject>.ByCallback providerJumpBoost = new (selector, Guid.NewGuid(), (po) => $"jump boost {((Player)po).jumpBoost}");
		realEntityMessages.AddDescriptor(providerRollCounter);
		realEntityMessages.AddDescriptor(providerJumpBoost);
	}
	public void Hook_NotifyUADDestroy(On.UpdatableAndDeletable.orig_Destroy orig, UpdatableAndDeletable self)
	{
		orig(self);
		if (destroyNotifyReceivers.TryGetValue(self, out IGetDestroyNotice<UpdatableAndDeletable> ign)) ign.ObjectDestroyed(self);
	}
}


