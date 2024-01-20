[assembly: System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]

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
	internal BepInEx.Configuration.ConfigEntry<bool>? cfgEnable;
	internal event Action<RainWorldGame>? OnRWGCreate;
	internal event Action<RainWorldGame, float>? OnRWGRawUpdate;
	internal event Action<RainWorldGame>? OnRWGUpdate;
	internal event Action<RainWorldGame>? OnRWGShutdown;
	internal Dictionary<(Type? visualizer, string id), Keybind> keybinds = new();
	internal DescriptorSet<AbstractWorldEntity> abstractEntityMessages = new();
	internal DescriptorSet<PhysicalObject> realEntityMessages = new();
	internal DescriptorSet<AbstractRoom> roomMessages = new();
	internal FContainer mainVisContainer = new();
	internal bool uiEnabled => cfgEnable?.Value ?? false;

	public void OnEnable()
	{
		__SwitchToBepinexLogger(Logger);
		On.RainWorldGame.ctor += Hook_RWGConstructor;
		On.RainWorldGame.RawUpdate += Hook_RWGRawUpdate;
		On.RainWorldGame.Update += Hook_RWGUpdate;
		On.RainWorldGame.ShutDownProcess += Hook_RWGShutdown;
		//On.RainWorld.Start += Hook_RWStart;
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
		cfgEnable = Config.Bind("general", "enable", false, "Enable the mod and list controls in options menu");

		VisualizerRealEntityMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + "/visPhysobj"));
		VisualizerRoomMessage.Init(new BepInEx.Logging.ManualLogSource(Logger.SourceName + "/visAbsRoom"));

		Selector<PhysicalObject>.Downcast<Player> selector = new();
		API.Labels.AddObjectLabel<Player>(player => $"jmp {player.jumpBoost}");
		API.Labels.AddObjectLabel<Player>(player => $"rll {player.rollCounter}");
		API.Labels.AddObjectLabel<Fly>(fly => fly.flap.ToString())
			.AddCondition(fly => !fly.dead);
		API.Labels.AddRoomLabel(room => $"ec {room.entities.Count}");
	}

	public Keybind GetKeybind<TVis>(TVis vis, string id, string desc, KeyCode def)
		where TVis : Visualizer<TVis>, new()
	{
		Type ttvis = typeof(TVis);
		if (!keybinds.TryGetValue((ttvis, id), out Keybind kb))
		{
			string section = vis.Name;
			kb = new(ttvis, Config.Bind(section, id, def, desc));
			keybinds[(ttvis, id)] = kb;
		}
		return kb;
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
			mainVisContainer.isVisible = cfgEnable?.Value ?? false;
			if (mainVisContainer.container is null)
			{
				try
				{
					Futile.stage.AddChild(mainVisContainer);
					if (mainVisContainer.container is null) throw new InvalidOperationException($"Parent is null!");
				}
				catch (Exception ex)
				{
					Logger.LogError($"Error adding visualizer container");
					Logger.LogError(ex);
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
			foreach (Keybind kb in keybinds.Values)
			{
				try
				{
					kb.Update();
				}
				catch (Exception ex)
				{
					LogError($"{kb} error on update : {ex}");
				}
			}
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

