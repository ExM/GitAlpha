using Avalonia.Markup.Xaml;

namespace MvvmDemo.Styles.Themes;

public class DefaultTheme : Avalonia.Styling.Styles
{
    public DefaultTheme() => AvaloniaXamlLoader.Load(this);
}
