using UnityEngine;

namespace BlackGlare;

/// <summary>
/// Preset selectors for physical objects.
/// </summary>
public abstract class SelectorPhysicalObject : Selector<PhysicalObject>
{
	/// <summary>
	/// Uses a unity collider to check chunk position.
	/// </summary>
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
