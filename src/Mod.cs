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

	public void OnEnable()
	{
		__SwitchToBepinexLogger(Logger);
		__writeCallsiteInfo = false;
		__writeTrace = true;
		VisualizerRealEntityMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + " Get Real"));
		VisualizerRoomMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + " Roomies"));
		Selector<PhysicalObject>.Downcast<Player> selector = new();
		API.Labels.AddObjectLabel<Player>(player => $"jmp {player.jumpBoost}");
		API.Labels.AddObjectLabel<Player>(player => $"rll {player.rollCounter}");
		API.Labels.AddObjectLabel<Fly>(fly => fly.flap.ToString())
			.AddCondition(fly => !fly.dead);
		API.Labels.AddRoomLabel(room => $"ec {room.entities.Count}");
	}
}


