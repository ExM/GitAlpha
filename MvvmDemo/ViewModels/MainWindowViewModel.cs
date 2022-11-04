using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
					nextRow.Transite = new List<ObjectId>();
				}
				else
				{
					var transite = new List<ObjectId>(prevRow.Transite);

					foreach (var id in prevRow.ParentIds)
					{
						if(!transite.Contains(id))
							transite.Add(id);
					}

					var foundIndex = transite.FindIndex(item => item == nextRow.Id);
					if(foundIndex != -1)
						transite.RemoveAt(foundIndex);
					
					nextRow.Transite = transite;
				}
				
				result.Add(nextRow);
				prevRow = nextRow;
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
