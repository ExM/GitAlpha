using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using GitAlpha.Extensions;
using GitAlpha.Git;
using ReactiveUI;

namespace MvvmDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
	private readonly Timer _timer;
	private string _greeting = "Welcome to Avalonia!";
	public string Greeting => _greeting;

	public MainWindowViewModel()
	{
		_timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

		var path = Directory.GetCurrentDirectory();

		while (true)
		{
			if (!Directory.Exists(path))
				throw new Exception($"directory {path} not exist");

			if (Directory.Exists(Path.Combine(path, ".git")))
				break;

			path = Path.GetDirectoryName(path);
		}

		var repo = new Repository(new DirectoryInfo(path));

		var gitRevs = repo.GetRevisions();


		var result = gitRevs.Select(revision => 
			new RevisionRow()
			{
				Id = revision.ObjectId,
				ParentIds = revision.ParentIds!,
				Author = revision.Author,
				Subject = revision.Subject,
				CommitDate = revision.CommitDate
			})
			.ToList();

		var first = result.FirstOrDefault();
		if (first is not null)
		{
			first.Render = new List<ObjectId>() { first.Id };
		}

		foreach (var pair in result.Zip(result.Skip(1), (a, b) => new {a, b}))
		{
			var uRow = pair.a;
			var dRow = pair.b;

			var uNodeIndex = uRow.Render.IndexOf(uRow.Id); // always exists
			var render = new List<ObjectId>(uRow.Render);
			render.RemoveAt(uNodeIndex);

			var missingParents = uRow.ParentIds.Where(parentId => !render.Contains(parentId)).ToList();

			bool foundParentNode = missingParents.Remove(dRow.Id);
			foreach (var parentId in missingParents)
				render.InsertOrAdd(uNodeIndex, parentId);

			if (foundParentNode)
				render.InsertOrAdd(uNodeIndex, dRow.Id);

			if (!render.Contains(dRow.Id))
			{
				var transitIndex = uRow.Render.IndexOf(dRow.Id);
				if (transitIndex != -1)
				{ // terminate transit if exists
					render.InsertOrAdd(transitIndex, dRow.Id);
				}
				else
				{ // new node
					render.Add(dRow.Id);
				}
			}

			dRow.Render = render;
			
			var nodeIndex = dRow.Render.IndexOf(dRow.Id);
			foreach (var (id, idx) in dRow.Render.Select((id, idx) => Tuple.Create(id, idx)))
			{
				if (dRow.ParentIds.Contains(id))
					dRow.MergeTransitRender.Add(new RevisionRow.MergeTransit(idx, nodeIndex, id));
			}

			var connect = new List<NodeConnection>();

			for(var uIdx = 0; uIdx < uRow.Render.Count; uIdx++)
			{
				var uId = uRow.Render[uIdx];
				if(uId == uRow.Id)
				{ // node & merge connections
					foreach (var pId in uRow.ParentIds)
					{
						var dIdx = dRow.Render.IndexOf(pId);
						if (dIdx != -1)
						{
							var mergeTransitIndex =
								uRow.MergeTransitRender.FindIndex(mT => mT.ParentId == pId);
							
							if (mergeTransitIndex != -1)
							{
								if (Math.Abs(uIdx - dIdx) <= 2)
								{
									uRow.MergeTransitRender.RemoveAt(mergeTransitIndex);
									connect.Add(new NodeConnection(pId, uIdx, dIdx));
								}
							}
							else
								connect.Add(new NodeConnection(pId, uIdx, dIdx));
						}
					}
				}
				else
				{ // transit connections
					var dIdx = dRow.Render.IndexOf(uId);
					if (dIdx != -1)
					{
						connect.Add(new NodeConnection(uId, uIdx, dIdx));
					}
				}
			}

			foreach (var (connId, uIdx, dIdx) in connect)
			{
				uRow.ConnectionsRender.Add(new RevisionRow.Connections(uIdx, dIdx - uIdx, connId, false));
				dRow.ConnectionsRender.Add(new RevisionRow.Connections(dIdx, uIdx - dIdx, connId, true));
			}
		}

		Revisions = result;
	}

	private record NodeConnection(ObjectId Id, int UpIndex, int DownIndex);

	public IReadOnlyList<RevisionRow> Revisions { get; }

	private void OnTimer(object? state)
	{
		_greeting = $"Welcome to Avalonia! {DateTime.Now:F}";

		this.RaisePropertyChanged(nameof(Greeting));
	}

	public void Dispose()
	{
		_timer.Dispose();
	}
}


