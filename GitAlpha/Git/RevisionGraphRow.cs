using System.Collections.Generic;

namespace GitAlpha.Git;

public class RevisionGraphRow
{
	public int NodeIndex { get; set; }

	public int ColorId { get; set; }
	
	public int AllNodes { get; set; }

	public List<Connections> ConnectionsRender { get; set; } = new List<Connections>();

	public record struct Connections
	{
		public int Index { get; init; }
		public int Delta { get; init; }
		public bool Up { get; init; }
		public int ColorId { get; init; }
	}
}
