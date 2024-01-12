namespace BlackGlare;

[BepInEx.BepInPlugin(ID, DISPLAYNAME, VERSION)]
public class Mod : BepInEx.BaseUnityPlugin
{
	public const string ID = "thalber.blackglare";
	public const string DISPLAYNAME = "Black Glare";
	public const string VERSION = "0.1";
	internal BepInEx.Configuration.ConfigEntry<bool>? cfgLFWriteTrace;
	internal BepInEx.Configuration.ConfigEntry<bool>? cfgLFWriteCallsite;
	internal BepInEx.Configuration.ConfigEntry<KeyCode>? cfgActivationKey;


	internal DescriptorSet<AbstractWorldEntity> abstractEntityMessages = new();
	internal DescriptorSet<PhysicalObject> realEntityMessages = new();
	internal DescriptorSet<AbstractRoom> roomMessages = new();

	public void OnEnable()
	{

		__SwitchToBepinexLogger(Logger);
		cfgLFWriteTrace = Config.Bind("logging", "writeTrace", false, "Write trace level log strings");
		cfgLFWriteTrace.SettingChanged += (sender, args) =>
		{
			__writeTrace = cfgLFWriteTrace.Value;
		};
		__writeTrace = cfgLFWriteTrace.Value;
		cfgLFWriteCallsite = Config.Bind("logging", "writeCallsite", false, "Write location of log calls");
		cfgLFWriteCallsite.SettingChanged += (sender, args) =>
		{
			__writeCallsiteInfo = cfgLFWriteCallsite.Value;
		};
		__writeCallsiteInfo = cfgLFWriteCallsite.Value;
		cfgActivationKey = Config.Bind("input", "mainActionKey", KeyCode.KeypadPlus);

		
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
