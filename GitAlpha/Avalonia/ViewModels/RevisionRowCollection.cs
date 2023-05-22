using System.Collections.ObjectModel;
using GitAlpha.Git;

namespace GitAlpha.Avalonia.ViewModels;

public class RevisionRowCollection: ObservableCollection<RevisionRow>
{
	public RevisionRowCollection(): base()
	{
	}
	
	public RevisionRowCollection(IEnumerable<RevisionRow> collection) : base(collection)
	{
	}

	public RevisionRowCollection(List<RevisionRow> list) : base(list)
	{
	}
}
