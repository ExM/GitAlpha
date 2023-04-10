namespace GitAlpha.Git;

public class ColorMap
{
	private int _current = 0;
	private readonly Dictionary<ObjectId, int> _map = new Dictionary<ObjectId, int>();

	public int this[ObjectId id] => _map[id];

	public int Map(ObjectId id)
	{
		if (_map.TryGetValue(id, out var knownColor))
		{
			return knownColor;
		}
	
		var result = _current;
		_map.Add(id, _current);
		_current++;
		return result;
	}

	public void SetOneKnown(ObjectId id, int color)
	{
		if(_map.ContainsKey(id))
			return;
			
		_map.Add(id, color);
	}
		
	public void SetMany(IEnumerable<ObjectId> ids)
	{
		foreach (var id in ids)
		{
			if (_map.ContainsKey(id))
				continue;
				
			_map.Add(id, _current);
			_current++;
		}
	}
}
