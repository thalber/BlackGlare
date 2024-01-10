namespace BlackGlare;

public interface IVisDynamicNode
{
	public bool slatedForDeletion { get; set; }
	public void Update(RoomCamera cam);
}
