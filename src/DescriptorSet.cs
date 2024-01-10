namespace BlackGlare;

public class DescriptorSet<TSubject>
{
	private readonly List<Descriptor<TSubject>> descriptors = new();

	public virtual IEnumerable<string> GetAllMessages(TSubject subject)
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
	public class WrapFrom<TWrap> : DescriptorSet<TWrap>
	{
		private readonly DescriptorSet<TSubject> actual;
		private readonly Func<TWrap, TSubject> convert;
		public WrapFrom(DescriptorSet<TSubject> actual, Func<TWrap, TSubject> convert)
		{
			this.actual = actual;
			this.convert = convert;
		}

		public override IEnumerable<string> GetAllMessages(TWrap subject)
		{
			return actual.GetAllMessages(convert(subject));
		}

	}
}
