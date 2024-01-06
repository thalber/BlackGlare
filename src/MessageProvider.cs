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

	public sealed class ByCallback : MessageProvider<TSubject>
	{
		private readonly Func<TSubject, string?> messageCallback;

		public ByCallback(Selector<TSubject> selector, Guid guid, Func<TSubject, string?> messageCallback) : base(selector, guid)
		{
			this.messageCallback = messageCallback;
		}
		public override string? GetMessage(TSubject item) => messageCallback(item);


	}
}
