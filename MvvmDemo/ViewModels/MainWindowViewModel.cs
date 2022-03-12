using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

            while(true)
            {
                if (!Directory.Exists(path))
                    throw new Exception($"directory {path} not exist");

                if (Directory.Exists(Path.Combine(path, ".git")))
                    break;

                path = Path.GetDirectoryName(path);
            }

            var repo = new Repository(new DirectoryInfo(path));

            Revisions = repo.GetRevisions();
        }

        public IReadOnlyList<Revision> Revisions { get; }

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
