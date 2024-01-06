namespace BlackGlare;

internal class MessageRegistry<TSubject>
{
	public readonly System.Collections.Generic.List<Descriptor<TSubject>> messageProviders = new();

	public System.Collections.Generic.IEnumerable<string> GetAllMessages(TSubject subject)
	{
		for (int i = 0; i < messageProviders.Count; i++)
		{
			Descriptor<TSubject> provider = messageProviders[i];
			if (
				provider.selector.Selected(subject)
				&& provider.GetMessage(subject) is string message
				)
			{
				yield return message;
			}
		}
	}
	public void AddProvider(Descriptor<TSubject> provider) => messageProviders.Add(provider);
	public bool RemoveProvider(System.Guid guid)
	{
		for (int i = 0; i < messageProviders.Count; i++)
		{
			Descriptor<TSubject> provider = messageProviders[i];
			if (provider.guid == guid)
			{
				messageProviders.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	public bool RemoveProvider(Descriptor<TSubject> provider) => messageProviders.Remove(provider);
}
