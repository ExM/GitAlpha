using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using GitAlpha.Git;

namespace MvvmDemo.Controls;

public class GraphRowControl : Control
{
	static GraphRowControl()
	{
	}

	public GraphRowControl()
	{
	}

	public RevisionRow? RevisionRow
	{
		get => _revisionRow;
		set => _revisionRow = value;
	}

	/// <summary>
	/// Defines the <see cref="Text"/> property.
	/// </summary>
	public static readonly DirectProperty<GraphRowControl, RevisionRow?> RevisionRowProperty =
		AvaloniaProperty.RegisterDirect<GraphRowControl, RevisionRow?>(
			nameof(RevisionRow),
			o => o.RevisionRow,
			(o, v) => o.RevisionRow = v);

	public override void Render(DrawingContext drawingContext)
	{
		if (_revisionRow is null)
			return;
		
		var p0 = new Point(0, 0);
		//var pB = new Point(p0.X + Bounds.Width, p0.Y + Bounds.Height);
		//drawingContext.DrawLine(new Pen(Brushes.Red, 3, lineCap: PenLineCap.Round), p0, pB);
		//drawingContext.DrawRectangle(new Pen(Brushes.Black), new Rect(p0, pB));

		

		var offset = 8;

		foreach (var id in _revisionRow.Transite)
		{
			var brush = GetBrush(id);
			drawingContext.DrawLine(new Pen(brush, 3), new Point(offset, 0), new Point(offset, Bounds.Height));
			offset += 10;
		}

		
		var brushId = GetBrush(_revisionRow.Id);

		drawingContext.DrawEllipse(brushId, new Pen(Brushes.Red, 1), new Point(offset, p0.Y + Bounds.Height/2), 5, 5);
	}

	private static ISolidColorBrush GetBrush(ObjectId id)
	{
		return _brushes[Math.Abs(id.GetHashCode()) % _brushes.Count];
	}

	private static readonly IList<ISolidColorBrush> _brushes = new List<ISolidColorBrush>()
	{
		Brushes.Blue,
		Brushes.Red,
		Brushes.Green,
		Brushes.Black,
		Brushes.Cyan,
		Brushes.Magenta
	};

	private RevisionRow? _revisionRow;
}
