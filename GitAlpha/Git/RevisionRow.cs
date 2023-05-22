namespace GitAlpha.Git
{
	public sealed class RevisionRow
	{
		public ObjectId Id { get; set; }
		public IReadOnlyList<ObjectId> ParentIds { get; set; }
		
		public List<ObjectId> Render { get; set; }

		public RevisionGraphRow Graph { get; set; } = new RevisionGraphRow();
		
		public string? Author { get; set; }
		public string? AuthorEmail { get; set; }

		public DateTime AuthorDate { get; set; }
		public string? Committer { get; set; }
		public string? CommitterEmail { get; set; }
		public DateTime CommitDate { get; set; }

		public string Subject { get; set; } = "";
	}
}
