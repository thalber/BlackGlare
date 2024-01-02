using UnityEngine;

namespace BlackGlare;

public abstract class SelectorPhysicalObject : Selector<PhysicalObject>
{
	
	public sealed class ByChunkPosition : SelectorPhysicalObject
	{
		private readonly int chunkIndex;
		private readonly Collider2D area;

		public ByChunkPosition(int chunkIndex, UnityEngine.Collider2D area)
		{
			this.chunkIndex = chunkIndex;
			this.area = area;
		}

		public override bool Selected(PhysicalObject item) => 
			(chunkIndex >= 0 && chunkIndex < item.bodyChunks.Length) 
			? area.OverlapPoint(item.bodyChunks[chunkIndex].pos) 
			: false;
	}
}
