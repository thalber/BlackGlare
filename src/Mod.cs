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

	public void OnEnable()
	{
	}
}
