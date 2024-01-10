using System;

namespace BlackGlare;

/// <summary>
/// Filters objects of a specified type. Nested types contain the pre-existing filters. See children <see cref="SelectorPhysicalObject"/> and <see cref="SelectorAbstractRoom"/> and their nested types for more case-specific presets.
/// </summary>
/// <typeparam name="TItem">Type of the objects the selector filters.</typeparam>
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
	public static Selector<TItem> operator &(Selector<TItem> a, Selector<TItem> b) => a.And(b);
	public static Selector<TItem> operator |(Selector<TItem> a, Selector<TItem> b) => a.Or(b);

	#region nested children presets
	/// <summary>
	/// Filters by casting objects to specified type. 
	/// </summary>
	/// <typeparam name="TDown">The type filtered object must belong to in order to pass. Can be child class of TItem or an interface.</typeparam>
	public sealed class Downcast<TDown> : Selector<TItem>
	{
		//TDown xd = default;
		public TDown getDown(TItem item) =>
			item is TDown down
			? down
			: throw new System.InvalidCastException($"{item} of type {item?.GetType().FullName} cannot be converted to {nameof(TDown)}");
		public override bool Selected(TItem item) => item is TDown;
	}
	/// <summary>
	/// Filters using a predicate.
	/// </summary>
	public sealed class ByCallback : Selector<TItem>
	{
		private readonly Func<TItem, bool> filterCallback;

		public ByCallback(System.Func<TItem, bool> filterCallback)
		{
			this.filterCallback = filterCallback;
		}
		public override bool Selected(TItem item) => filterCallback(item);
	}
	/// <summary>
	/// Filters by downcasting to specified type and passing result through a nested selector for that type.
	/// </summary>
	/// <typeparam name="TDown">The type filtered object must belong to in order to pass. Can be child class of TItem or an interface.</typeparam>
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
	/// <summary>
	/// Filters by downcasting to specified type and passing result through a predicate.
	/// </summary>
	/// <typeparam name="TDown">The type filtered object must belong to in order to pass. Can be child class of TItem or an interface.</typeparam>
	public sealed class ByDowncastCallback<TDown> : Selector<TItem>
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
	/// <summary>
	/// Applies logical OR to two selectors, short-circuiting.
	/// </summary>
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
	/// <summary>
	/// Applies logical AND to two selectors, short-circuiting.
	/// </summary>
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
	/// <summary>
	/// Selects everything.
	/// </summary>
	public sealed class All : Selector<TItem>
	{
		public override bool Selected(TItem item) => true;
	}
	/// <summary>
	/// Selects nothing.
	/// </summary>
	public sealed class None : Selector<TItem>
	{
		public override bool Selected(TItem item) => false;
	}
	#endregion
}
