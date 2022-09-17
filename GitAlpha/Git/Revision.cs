using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace GitAlpha.Git
{
	public sealed class Revision
	{
		public string Id { get; set; }
		public IReadOnlyList<string> ParentIds { get; set; }

		public string? Author { get; set; }
		public string? AuthorEmail { get; set; }

		public DateTime AuthorDate { get; set; }
		public string? Committer { get; set; }
		public string? CommitterEmail { get; set; }
		public DateTime CommitDate { get; set; }

		public string Subject { get; set; } = "";
	}
}
