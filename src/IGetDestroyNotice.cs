namespace BlackGlare;

public interface IGetDestroyNotice<TEntity>
{
	public void ObjectDestroyed(TEntity thing);
}