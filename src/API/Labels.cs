namespace BlackGlare.API;

/// <summary>
/// Use static methods here to add labels to things.
/// </summary>
public static class Labels
{
	/// <summary>
	/// Adds a label to specified type of physicalobject.
	/// </summary>
	/// <param name="describe">Callback that returns the label text.</param>
	/// <typeparam name="TObj">Type of physicalobject that should have the label. Can be an interface.</typeparam>
	/// <returns>LabelDefinition object that can be used to add extra conditions.</returns>
	public static LabelDefinition<PhysicalObject, TObj> AddObjectLabel<TObj>(Func<TObj, string?> describe)
		where TObj : notnull
	{
		PluginNotInitializedException.ThrowIfNotInitialized();
		LabelDefinition<PhysicalObject, TObj> result = new(describe);
		Mod mod = UnityEngine.Object.FindObjectOfType<Mod>();
		mod.realEntityMessages.AddDescriptor(result.actual);
		return result;
	}
	/// <summary>
	/// Adds a label to room panels in devtools devmap view.
	/// </summary>
	/// <param name="describe">Callback that returns the label text.</param>
	/// <returns>LabelDefinition object that can be used to add extra conditions.</returns>
	public static LabelDefinition<AbstractRoom, AbstractRoom> AddRoomLabel(Func<AbstractRoom, string?> describe)
	{
		PluginNotInitializedException.ThrowIfNotInitialized();
		LabelDefinition<AbstractRoom, AbstractRoom> result = new(describe);
		Mod mod = UnityEngine.Object.FindObjectOfType<Mod>();
		mod.roomMessages.AddDescriptor(result.actual);
		return result;
	}

}
