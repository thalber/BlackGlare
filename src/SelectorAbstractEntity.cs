using System;

namespace BlackGlare;

public abstract class SelectorAbstractEntity : Selector<AbstractWorldEntity>
{
	public sealed class ByAPOType : SelectorAbstractEntity
	{
		private readonly AbstractPhysicalObject.AbstractObjectType type;

		public ByAPOType(AbstractPhysicalObject.AbstractObjectType type)
		{
			this.type = type;
		}
		public override bool Selected(AbstractWorldEntity entity) => entity is AbstractPhysicalObject apo && apo.type == type;
	}
	public sealed class ByCreatureTemplate : SelectorAbstractEntity
	{
		private readonly CreatureTemplate.Type type;

		public ByCreatureTemplate(CreatureTemplate.Type type)
		{
			this.type = type;
		}
		public override bool Selected(AbstractWorldEntity entity) => entity is AbstractCreature crit && crit.creatureTemplate.type == type;
	}
	public sealed class ByRealizedSelector : SelectorAbstractEntity
	{
		private readonly SelectorPhysicalObject realFilter;
		public ByRealizedSelector(SelectorPhysicalObject realFilter) => this.realFilter = realFilter;
		public override bool Selected(AbstractWorldEntity item) => item is AbstractPhysicalObject apo && apo.realizedObject is PhysicalObject po && realFilter.Selected(po);
	}
}
