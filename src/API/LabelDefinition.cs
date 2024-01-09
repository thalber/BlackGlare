namespace BlackGlare.API;

public class LabelDefinition<TItem, TDown>
	where TItem : notnull
	where TDown : notnull
{
	internal Descriptor<TItem> actual;
	internal Selector<TDown> innerSelector;
	public LabelDefinition(Func<TDown, string?> describe)
	{
		innerSelector = new Selector<TDown>.All();
		var selector = new Selector<TItem>.ByDowncastChain<TDown>(innerSelector);
		actual = new Descriptor<TItem>.ByCallback(
			selector,
			Guid.NewGuid(),
			(obj) => describe((TDown)(object)obj));
	}
	public LabelDefinition<TItem, TDown> AddCondition(Func<TDown, bool> condition)
	{
		innerSelector &= new Selector<TDown>.ByCallback(condition);
		actual.selector = new Selector<TItem>.ByDowncastChain<TDown>(innerSelector);
		return this;
	}
}
