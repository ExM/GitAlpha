using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using GitAlpha.Avalonia.Converters;
using GitAlpha.Git;

namespace GitAlpha.Avalonia.Controls;

public partial class RevisionListControl : UserControl
{
	private readonly ObjectIdRenderer _objectIdRenderer;

	public RevisionListControl()
    {
        InitializeComponent();
        _objectIdRenderer = (ObjectIdRenderer)Resources["ObjectIdRenderer"]!;
    }
	
	public static readonly DirectProperty<RevisionListControl, int> IdLengthProperty =
		AvaloniaProperty.RegisterDirect<RevisionListControl, int>(
			nameof(IdLength),
			o => o.IdLength,
			(o, v) => o.IdLength = v);

	public int IdLength
	{
		get => _objectIdRenderer.Length;
		set
		{
			_objectIdRenderer.Length = value;

			foreach (var idView in ListBox.GetLogicalDescendants().OfType<TextBlock>().Where(tb => tb.Name == "IdView"))
			{
				if(idView.DataContext is RevisionRow row)
					idView.Text = _objectIdRenderer.Render(row.Id);
			}
		}
	}
}
