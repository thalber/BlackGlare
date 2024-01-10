using System;

namespace BlackGlare;

/// <summary>
/// Preset selectors for AWEs.
/// </summary>
public abstract class SelectorAbstractEntity : Selector<AbstractWorldEntity>
{
	/// <summary>
	/// Downcasts to <see cref="AbstractPhysicalObject"/> and checks its type field.
	/// </summary>
	public sealed class ByAPOType : SelectorAbstractEntity
	{
		private readonly AbstractPhysicalObject.AbstractObjectType type;

		public ByAPOType(AbstractPhysicalObject.AbstractObjectType type)
		{
			this.type = type;
		}
		public override bool Selected(AbstractWorldEntity entity) => entity is AbstractPhysicalObject apo && apo.type == type;
	}
	/// <summary>
	/// Downcasts to <see cref="AbstractCreature"/> and checks its template type.
	/// </summary>
	public sealed class ByCreatureTemplate : SelectorAbstractEntity
	{
		private readonly CreatureTemplate.Type type;

		public ByCreatureTemplate(CreatureTemplate.Type type)
		{
			this.type = type;
		}
		public override bool Selected(AbstractWorldEntity entity) => entity is AbstractCreature crit && crit.creatureTemplate.type == type;
	}
	/// <summary>
	/// Downcasts to <see cref="AbstractCreature"/>, checks if realized and if realized passes a predicate.
	/// </summary>
	public sealed class ByRealizedSelector : SelectorAbstractEntity
	{
		private readonly SelectorPhysicalObject realFilter;
		public ByRealizedSelector(SelectorPhysicalObject realFilter) => this.realFilter = realFilter;
		public override bool Selected(AbstractWorldEntity item) => item is AbstractPhysicalObject apo && apo.realizedObject is PhysicalObject po && realFilter.Selected(po);
	}
}
