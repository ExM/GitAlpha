using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MvvmDemo.ViewModels;
using MvvmDemo.Views;

namespace MvvmDemo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var model = new MainWindowViewModel();
                
                desktop.MainWindow = new MainWindow
                {
                    DataContext = model,
                };

                desktop.ShutdownRequested += (sender, args) =>
                {
                    model.Dispose();
                };
            }
            
            base.OnFrameworkInitializationCompleted();
        }
    }
}