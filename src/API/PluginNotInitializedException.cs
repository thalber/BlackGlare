namespace BlackGlare;

public sealed class PluginNotInitializedException : Exception
{
	public static void ThrowIfNotInitialized()
	{
		if (UnityEngine.Object.FindObjectsOfType<Mod>().Length == 0) throw new PluginNotInitializedException();
	}
	public override string Message => $"{Mod.DISPLAYNAME} has not been initialized. Add BepInDependency for {Mod.ID} to ensure the correct load order.";
}