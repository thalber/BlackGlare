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
	internal event Action<RainWorldGame>? OnRWGCreate;
	internal event Action<RainWorldGame, float>? OnRWGRawUpdate;
	internal event Action<RainWorldGame>? OnRWGUpdate;
	internal event Action<RainWorldGame>? OnRWGShutdown;

	internal DescriptorSet<AbstractWorldEntity> abstractEntityMessages = new();
	internal DescriptorSet<PhysicalObject> realEntityMessages = new();
	internal DescriptorSet<AbstractRoom> roomMessages = new();

	public void OnEnable()
	{

		__SwitchToBepinexLogger(Logger);
		On.RainWorldGame.ctor += Hook_RWGConstructor;
		On.RainWorldGame.RawUpdate += Hook_RWGRawUpdate;
		On.RainWorldGame.Update += Hook_RWGUpdate;
		On.RainWorldGame.ShutDownProcess += Hook_RWGShutdown;
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

		VisualizerRealEntityMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + "/visPhysobj"));
		VisualizerRoomMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + "/visAbsRoom"));

		Selector<PhysicalObject>.Downcast<Player> selector = new();
		API.Labels.AddObjectLabel<Player>(player => $"jmp {player.jumpBoost}");
		API.Labels.AddObjectLabel<Player>(player => $"rll {player.rollCounter}");
		API.Labels.AddObjectLabel<Fly>(fly => fly.flap.ToString())
			.AddCondition(fly => !fly.dead);
		API.Labels.AddRoomLabel(room => $"ec {room.entities.Count}");
	}


	private void Hook_RWGConstructor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
	{
		try
		{
			orig(self, manager);
		}
		finally
		{
			foreach (Action<RainWorldGame> sub in (OnRWGCreate?.GetInvocationList() ?? new Delegate[0]))
			{
				try
				{
					sub?.Invoke(self);
				}
				catch (Exception ex)
				{
					LogError($"UNHANDLED EXCEPTION IN {sub.Method} ON {nameof(Hook_RWGConstructor)}");
					LogError(ex);
				}
			}
		}
	}
	private void Hook_RWGRawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float delta)
	{
		try
		{
			orig(self, delta);
		}
		finally
		{
			foreach (Action<RainWorldGame, float> sub in (OnRWGRawUpdate?.GetInvocationList() ?? new Delegate[0]))
			{
				try
				{
					sub?.Invoke(self, delta);
				}
				catch (Exception ex)
				{
					LogError($"UNHANDLED EXCEPTION IN {sub.Method} ON {nameof(Hook_RWGRawUpdate)}");
					LogError(ex);
				}
			}
		}
	}
	private void Hook_RWGUpdate(On.RainWorldGame.orig_Update orig, RainWorldGame self)
	{
		try
		{
			orig(self);
		}
		finally
		{
			foreach (Action<RainWorldGame> sub in (OnRWGUpdate?.GetInvocationList() ?? new Delegate[0]))
			{
				try
				{
					sub?.Invoke(self);
				}
				catch (Exception ex)
				{
					LogError($"UNHANDLED EXCEPTION IN {sub.Method} ON {nameof(Hook_RWGUpdate)}");
					LogError(ex);
				}
			}
		}
	}
	private void Hook_RWGShutdown(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
	{
		try
		{
			orig(self);
		}
		finally
		{

			foreach (Action<RainWorldGame> sub in (OnRWGShutdown?.GetInvocationList() ?? new Delegate[0]))
			{
				try
				{
					sub?.Invoke(self);
				}
				catch (Exception ex)
				{
					LogError($"UNHANDLED EXCEPTION IN {sub.Method} ON {nameof(Hook_RWGShutdown)}");
					LogError(ex);
				}
			}
		}
	}
}
