using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace GitAlpha.Git
{
	public sealed class RevisionRow
	{
		public ObjectId Id { get; set; }
		public IReadOnlyList<ObjectId> ParentIds { get; set; }
		
		public List<ObjectId> Render { get; set; }
		
		public int ColorId { get; set; }

		public List<Connections> ConnectionsRender { get; set; } = new List<Connections>();
		
		public string? Author { get; set; }
		public string? AuthorEmail { get; set; }

		public DateTime AuthorDate { get; set; }
		public string? Committer { get; set; }
		public string? CommitterEmail { get; set; }
		public DateTime CommitDate { get; set; }

		public string Subject { get; set; } = "";
		public int NodeIndex { get; set; }

		public record struct Connections
		{
			public int Index { get; init; }
			public int Delta { get; init; }
			public bool Up { get; init; }
			public int ColorId { get; init; }
		}
	}
}
