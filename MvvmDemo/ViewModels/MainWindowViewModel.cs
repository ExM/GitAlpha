using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
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
            
            
        }

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
