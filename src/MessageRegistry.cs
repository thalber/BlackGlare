namespace BlackGlare;

internal class MessageRegistry<TSubject>
{
	public readonly System.Collections.Generic.List<MessageProvider<TSubject>> messageProviders = new();

	public System.Collections.Generic.IEnumerable<string> GetAllMessages(TSubject subject)
	{
		for (int i = 0; i < messageProviders.Count; i++)
		{
			MessageProvider<TSubject> provider = messageProviders[i];
			if (
				provider.selector.Selected(subject)
				&& provider.GetMessage(subject) is string message
				)
			{
				yield return message;
			}
		}
	}
	public void AddProvider(MessageProvider<TSubject> provider) => messageProviders.Add(provider);
	public bool RemoveProvider(System.Guid guid)
	{
		for (int i = 0; i < messageProviders.Count; i++)
		{
			MessageProvider<TSubject> provider = messageProviders[i];
			if (provider.guid == guid)
			{
				messageProviders.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	public bool RemoveProvider(MessageProvider<TSubject> provider) => messageProviders.Remove(provider);
}
