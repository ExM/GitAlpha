using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
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

	public static readonly DirectProperty<GraphRowControl, RevisionRow?> RevisionRowProperty =
		AvaloniaProperty.RegisterDirect<GraphRowControl, RevisionRow?>(
			nameof(RevisionRow),
			o => o.RevisionRow,
			(o, v) => o.RevisionRow = v);

	public int LeftMargin { get; set; } = 8;
	
	public int NodeInterval { get; set; } = 16;
	
	public double NodeSize { get; set; } = 5;

	public override void Render(DrawingContext drawingContext)
	{
		if (_revisionRow is null)
			return;

		var halfHeight = Bounds.Height / 2;

		foreach (var conn in _revisionRow.ConnectionsRender)
		{
			var pen = GetPen(conn.ConnId);
			var baseX = LeftMargin + NodeInterval * conn.Index;
			var targetX = baseX + conn.Delta * NodeInterval / 2;
			
			if (conn.Up)
			{
				drawingContext.DrawGeometry(null, pen, new PathGeometry
				{
					Figures = new PathFigures
					{
						new PathFigure
						{
							StartPoint = new Point(baseX, halfHeight),
							Segments = new PathSegments()
							{
								new BezierSegment
								{
									Point1 = new Point(baseX, halfHeight),
									Point2 = new Point(baseX, halfHeight / 2),
									Point3 = new Point(targetX, 0)
								},
							},
							IsClosed = false,
							IsFilled = false
						}
					}
				});
			}
			else
			{
				drawingContext.DrawGeometry(null, pen, new PathGeometry
				{
					Figures = new PathFigures
					{
						new PathFigure
						{
							StartPoint = new Point(baseX, halfHeight),
							Segments = new PathSegments()
							{
								new BezierSegment
								{
									Point1 = new Point(baseX, halfHeight),
									Point2 = new Point(baseX, halfHeight + halfHeight / 2),
									Point3 = new Point(targetX, Bounds.Height)
								},
							},
							IsClosed = false,
							IsFilled = false
						}
					}
				});
			}
		}

		var nodeBrush = GetBrush(_revisionRow.Id);
		var nodeIndex = _revisionRow.Render.IndexOf(_revisionRow.Id);
		
		drawingContext.DrawEllipse(nodeBrush, null, new Point(
			LeftMargin + nodeIndex * NodeInterval, halfHeight), NodeSize, NodeSize);
	}

	private static ISolidColorBrush GetBrush(ObjectId id)
	{
		return _brushes[Math.Abs(id.GetHashCode()) % _brushes.Count];
	}
	
	private static Pen GetPen(ObjectId id)
	{
		return _pens[Math.Abs(id.GetHashCode()) % _brushes.Count];
	}
	
	private static readonly IList<Color> _colorPreset = new List<Color>()
	{
		Color.FromRgb(240, 100, 160), // red-pink
		Color.FromRgb(120, 180, 230), // light blue
		Color.FromRgb(36, 194, 33), // green
		Color.FromRgb(160, 120, 240), // light violet
		Color.FromRgb(221, 50, 40), // red
		Color.FromRgb(26, 198, 166), // cyan-green
		Color.FromRgb(231, 176, 15) // orange
	};
	
	private static readonly IList<ISolidColorBrush> _brushes =
		_colorPreset.Select(color => (ISolidColorBrush)new ImmutableSolidColorBrush(color)).ToList();

	private static readonly IList<Pen> _pens =
		_brushes.Select(brush => new Pen(brush, 2, null, PenLineCap.Round)).ToList();

	private RevisionRow? _revisionRow;
}
