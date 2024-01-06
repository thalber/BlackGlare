using System;

namespace BlackGlare;

public abstract class Descriptor<TSubject>
{
	public readonly Selector<TSubject> selector;
	public readonly Guid guid;

	public Descriptor(Selector<TSubject> selector, System.Guid guid)
	{
		this.selector = selector;
		this.guid = guid;
	}
	public virtual string? GetMessage(TSubject item) => item?.ToString() ?? null;

	public sealed class ByCallback : Descriptor<TSubject>
	{
		private readonly Func<TSubject, string?> messageCallback;

		public ByCallback(Selector<TSubject> selector, Guid guid, Func<TSubject, string?> messageCallback) : base(selector, guid)
		{
			this.messageCallback = messageCallback;
		}
		public override string? GetMessage(TSubject item) => messageCallback(item);


	}
}
