using System;

namespace BlackGlare;

public abstract class Selector<TItem>
{
	public abstract bool Selected(TItem item);
	public Selector<TItem> And<TOther>(TOther other)
		where TOther : Selector<TItem>
	{
		return new JoinAnd(this, other);
	}
	public Selector<TItem> Or<TOther>(TOther other)
		where TOther : Selector<TItem>
	{
		return new JoinOr(this, other);
	}
	public static Selector<TItem> operator &(Selector<TItem> a, Selector<TItem> b)
	{
		return a.And(b);
	}
	public static Selector<TItem> operator |(Selector<TItem> a, Selector<TItem> b)
	{
		return a.Or(b);
	}

	public class Downcast<TDown> : Selector<TItem>
	{
		//TDown xd = default;
		public TDown getDown(TItem item) =>
			item is TDown down
			? down
			: throw new System.InvalidCastException($"{item} of type {item?.GetType().FullName} cannot be converted to {nameof(TDown)}");
		public override bool Selected(TItem item) => item is TDown;
	}
	public sealed class ByCallback : Selector<TItem>
	{
		private readonly Func<TItem, bool> filterCallback;

		public ByCallback(System.Func<TItem, bool> filterCallback)
		{
			this.filterCallback = filterCallback;
		}
		public override bool Selected(TItem item) => filterCallback(item);
	}
	public sealed class ByDowncastChain<TDown> : Selector<TItem>
	{
		private readonly Selector<TDown> chain;
		public ByDowncastChain(Selector<TDown> chained)
		{
			this.chain = chained;
		}
		public override bool Selected(TItem item)
		{
			return item is TDown down && chain.Selected(down);
		}
	}
	public sealed class ByDowncastCallback<TDown> : Downcast<TDown>
	{
		private readonly Func<TDown, bool> filterCallback;

		public ByDowncastCallback(System.Func<TDown, bool> filterCallback)
		{
			this.filterCallback = filterCallback;
		}
		public override bool Selected(TItem item)
		{
			return item is TDown downcast && filterCallback(downcast);
		}
	}
	public sealed class JoinOr : Selector<TItem>
	{
		private readonly Selector<TItem> l;
		private readonly Selector<TItem> r;

		public JoinOr(Selector<TItem> l, Selector<TItem> r)
		{
			this.l = l;
			this.r = r;
		}
		public override bool Selected(TItem item) => l.Selected(item) || r.Selected(item);
	}
	public sealed class JoinAnd : Selector<TItem>
	{
		private readonly Selector<TItem> l;
		private readonly Selector<TItem> r;

		public JoinAnd(Selector<TItem> l, Selector<TItem> r)
		{
			this.l = l;
			this.r = r;
		}
		public override bool Selected(TItem item) => l.Selected(item) && r.Selected(item);
	}
	public sealed class All : Selector<TItem>
	{
		public override bool Selected(TItem item) => true;
	}
	public sealed class None : Selector<TItem>
	{
		public override bool Selected(TItem item) => false;
	}
}
