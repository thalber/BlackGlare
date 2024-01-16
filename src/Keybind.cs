namespace BlackGlare;

public record Keybind(Type vis, BepInEx.Configuration.ConfigEntry<KeyCode> cfg)
{
	private const int BUFFER_SIZE = 16;
	private readonly bool[] _buffer = new bool[BUFFER_SIZE];
	public bool RawGetDown => Input.GetKeyDown(cfg.Value);
	public bool RawGetUp => Input.GetKeyUp(cfg.Value);
	public bool RawGet => Input.GetKey(cfg.Value);
	public bool LoopGetDown => _buffer[0] && !_buffer[1];
	public bool LoopGetUp => !_buffer[0] && _buffer[1];
	internal void Update()
	{
		for (int i = 1; i < BUFFER_SIZE; i++)
		{
			_buffer[i] = _buffer[i - 1];
		}
		_buffer[0] = RawGet;
	}
}
