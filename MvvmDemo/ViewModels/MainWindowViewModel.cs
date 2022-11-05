using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DynamicData;
using GitAlpha.Git;
using ReactiveUI;

namespace MvvmDemo.ViewModels
{
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

			var result = new List<RevisionRow>();

			var gitRevs = repo.GetRevisions();

			RevisionRow? prevRow = null;
			
			foreach (var revision in gitRevs)
			{
				var nextRow = new RevisionRow()
				{
					Id = revision.ObjectId,
					ParentIds = revision.ParentIds!,
					Author = revision.Author,
					Subject = revision.Subject,
					CommitDate = revision.CommitDate
				};

				if (prevRow is null)
				{
					nextRow.Transit = new List<ObjectId>();
				}
				else
				{
					var transit = new List<ObjectId>(prevRow.Transit);

					foreach (var id in prevRow.ParentIds)
					{
						if(!transit.Contains(id))
							transit.Add(id);
					}

					var foundIndex = transit.FindIndex(item => item == nextRow.Id);
					if(foundIndex != -1)
						transit.RemoveAt(foundIndex);
					
					nextRow.Transit = transit;
				}
				
				result.Add(nextRow);
				prevRow = nextRow;
			}

			var first = result.FirstOrDefault();
			if (first is not null)
			{
				first.Render = new List<ObjectId>() { first.Id };
			}

			foreach (var pair in result.Zip(result.Skip(1), (a, b) => new {a, b}))
			{
				var uRow = pair.a;
				var dRow = pair.b;

				var render = new List<ObjectId>(dRow.Transit);

				var transitIndex = uRow.Render.IndexOf(dRow.Id);
				if (transitIndex != -1)
				{
					if(transitIndex < render.Count)
						render.Insert(transitIndex, dRow.Id);
					else
					{
						render.Add(dRow.Id); 
					}
				}
				else
				{
					if (uRow.ParentIds.Contains(dRow.Id))
					{
						var uNodeIndex = uRow.Render.IndexOf(uRow.Id);
						// if (uNodeIndex != -1) always true
						if (uNodeIndex < render.Count)
							render.Insert(uNodeIndex, dRow.Id);
						else
						{
							render.Add(dRow.Id);
						}
					}
					else
					{
						render.Add(dRow.Id);
					}
				}

				dRow.Render = render;

				var connect = new List<Tuple<int, int, ObjectId>>();

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
								connect.Add(Tuple.Create(uIdx, dIdx, pId));
							}
						}
					}
					else
					{ // transit connections
						var dIdx = dRow.Render.IndexOf(uId);
						if (dIdx != -1)
						{
							connect.Add(Tuple.Create(uIdx, dIdx, uId));
						}
					}
				}
				
				foreach (var (uIdx, dIdx, connId) in connect)
				{
					uRow.ConnectionsRender.Add(new RevisionRow.Connections(uIdx, dIdx - uIdx, connId, false));
					dRow.ConnectionsRender.Add(new RevisionRow.Connections(dIdx, uIdx - dIdx, connId, true));
				}
				
				var nodeIndex = dRow.Render.IndexOf(dRow.Id);
				foreach (var (id, idx) in dRow.Render.Select((id, idx) => Tuple.Create(id, idx)))
				{
					if (dRow.ParentIds.Contains(id))
					{
						if(!connect.Any(tuple => tuple.Item3 == id)) // remove merge if connection already exists 
							dRow.MergeTransitRender.Add(new RevisionRow.MergeTransit(idx, nodeIndex));
					}
				}
			}

			Revisions = result;
		}

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
}
