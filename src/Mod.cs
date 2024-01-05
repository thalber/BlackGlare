using System;
using System.Collections;
using System.Collections.Generic;

namespace BlackGlare;

[BepInEx.BepInPlugin("thalber.blackglare", "Black Glare", "0.1")]
public class Mod : BepInEx.BaseUnityPlugin
{
	//internal BepInEx.Configuration.ConfigEntry<bool> enableUI;
	internal MessageRegistry<AbstractWorldEntity> abstractEntityMessages = new();
	internal MessageRegistry<PhysicalObject> realEntityMessages = new();
	internal MessageRegistry<AbstractRoom> roomMessages = new();

	internal System.Runtime.CompilerServices.ConditionalWeakTable<UpdatableAndDeletable, IGetDestroyNotice<UpdatableAndDeletable>> destroyNotifyReceivers = new();

	public void OnEnable()
	{
		__SwitchToBepinexLogger(Logger);
		__writeCallsiteInfo = false;
		__writeTrace = true;
		VisualizerRealEntityMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + "GetReal"));
		On.UpdatableAndDeletable.Destroy += Hook_NotifyUADDestroy;
	}
	public void Hook_NotifyUADDestroy(On.UpdatableAndDeletable.orig_Destroy orig, UpdatableAndDeletable self)
	{
		orig(self);
		if (destroyNotifyReceivers.TryGetValue(self, out IGetDestroyNotice<UpdatableAndDeletable> ign)) ign.ObjectDestroyed(self);
	}
}


