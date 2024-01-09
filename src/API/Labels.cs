namespace BlackGlare.API;

public static class Labels
{
	public static LabelDefinition<PhysicalObject, TObj> AddObjectLabel<TObj>(Func<TObj, string?> describe)
		where TObj : notnull
	{
		PluginNotInitializedException.ThrowIfNotInitialized();
		LabelDefinition<PhysicalObject, TObj> result = new(describe);
		Mod mod = UnityEngine.Object.FindObjectOfType<Mod>();
		mod.realEntityMessages.AddDescriptor(result.actual);
		return result;
	}
	public static LabelDefinition<AbstractRoom, AbstractRoom> AddRoomLabel(Func<AbstractRoom, string?> describe)
	{
		PluginNotInitializedException.ThrowIfNotInitialized();
		LabelDefinition<AbstractRoom, AbstractRoom> result = new(describe);
		Mod mod = UnityEngine.Object.FindObjectOfType<Mod>();
		mod.roomMessages.AddDescriptor(result.actual);
		return result;
	}

}
