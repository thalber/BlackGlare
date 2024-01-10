namespace BlackGlare;

/// <summary>
/// Preset selectors for abstract rooms.
/// </summary>
public abstract class SelectorAbstractRoom : Selector<AbstractRoom>
{
	//todo: more builtin filters
	/// <summary>
	/// Selects rooms that have a specific tag.
	/// </summary>
	public sealed class ByTag : SelectorAbstractRoom
	{
		private readonly string tag;

		public ByTag(string tag)
		{
			this.tag = tag;
		}

		public override bool Selected(AbstractRoom item) => item.roomTags.Contains(tag);
	}
}
