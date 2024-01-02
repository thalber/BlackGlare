namespace BlackGlare;

public abstract class SelectorAbstractRoom : Selector<AbstractRoom>
{
	//todo: more builtin filters
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
