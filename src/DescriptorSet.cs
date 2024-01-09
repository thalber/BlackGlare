namespace BlackGlare;

internal class DescriptorSet<TSubject>
{
	private readonly List<Descriptor<TSubject>> descriptors = new();

	public IEnumerable<string> GetAllMessages(TSubject subject)
	{
		for (int i = 0; i < descriptors.Count; i++)
		{
			Descriptor<TSubject> provider = descriptors[i];
			if (
				provider.selector.Selected(subject)
				&& provider.GetMessage(subject) is string message
				)
			{
				yield return message;
			}
		}
	}
	public void AddDescriptor(Descriptor<TSubject> descriptor) => descriptors.Add(descriptor);
	public bool RemoveDescriptor(Guid guid)
	{
		for (int i = 0; i < descriptors.Count; i++)
		{
			Descriptor<TSubject> provider = descriptors[i];
			if (provider.guid == guid)
			{
				descriptors.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	public bool RemoveDescriptor(Descriptor<TSubject> descriptor) => descriptors.Remove(descriptor);
}
