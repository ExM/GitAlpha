using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace MvvmDemo.Controls;

public class GraphRowControl : Control
{
	static GraphRowControl()
	{
	}

	public GraphRowControl()
	{
	}

	public override void Render(DrawingContext drawingContext)
	{
		var p0 = new Point(0, 0);
		var pB = new Point(p0.X + Bounds.Width, p0.Y + Bounds.Height);

		drawingContext.DrawLine(new Pen(Brushes.Red, 3, lineCap: PenLineCap.Round), p0, pB);
		drawingContext.DrawRectangle(new Pen(Brushes.Black), new Rect(p0, pB));
	}
}
