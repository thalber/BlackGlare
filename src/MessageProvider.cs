using System;

namespace BlackGlare;

public abstract class MessageProvider<TSubject>
{
	public readonly Selector<TSubject> selector;
	public readonly Guid guid;

	public MessageProvider(Selector<TSubject> selector, System.Guid guid)
	{
		this.selector = selector;
		this.guid = guid;
	}
	public virtual string? GetMessage(TSubject item) => item?.ToString() ?? null;

	// public class Downcast<TDown> : MessageProvider<TSubject>
	// {
	// 	public Downcast(Selector<TSubject> selector, Guid guid) : base(selector, guid)
	// 	{
	// 	}
	// 	public override string? GetMessage(TSubject item)
	// 	{
	// 		return base.GetMessage(item);
	// 	}
	// }
}
