using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using GitAlpha.Git;

namespace MvvmDemo.Controls;

public class GraphRowControl : Control
{
	static GraphRowControl()
	{
	}

	public RevisionGraphRow? GraphRow
	{
		get
		{
			return _revisionRow;
		}
		set
		{
			_revisionRow = value;
			if (_revisionRow != null)
			{
				Width = LeftMargin + _revisionRow.AllNodes * NodeInterval;
			}
		}
	}

	public static readonly DirectProperty<GraphRowControl, RevisionGraphRow?> GraphRowProperty =
		AvaloniaProperty.RegisterDirect<GraphRowControl, RevisionGraphRow?>(
			nameof(RevisionRow),
			o => o.GraphRow,
			(o, v) => o.GraphRow = v);
	
	public static readonly DirectProperty<GraphRowControl, double?> RenderHeightProperty =
		AvaloniaProperty.RegisterDirect<GraphRowControl, double?>(
			nameof(RenderHeight),
			o => o.RenderHeight,
			(o, v) => o.RenderHeight = v);

	public int LeftMargin { get; set; } = 8;
	
	public int NodeInterval { get; set; } = 16;
	
	public double NodeSize { get; set; } = 5;
	
	public double? RenderHeight { get; set; }

	public override void Render(DrawingContext drawingContext)
	{
		if (_revisionRow is null)
			return;

		var height = RenderHeight ?? Bounds.Height;
		
		var halfHeight = height / 2;

		foreach (var conn in _revisionRow.ConnectionsRender)
		{
			var pen = GetPen(conn.ColorId);
			var baseX = LeftMargin + NodeInterval * conn.Index;
			var targetX = baseX + conn.Delta * NodeInterval / 2;
			var targetX2 = baseX + conn.Delta * NodeInterval;
			
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
									Point2 = new Point(baseX, halfHeight / 4),
									Point3 = new Point(targetX, 0)
								},
								new BezierSegment
								{
									Point1 = new Point(targetX, 0),
									Point2 = new Point(targetX2, -halfHeight / 4),
									Point3 = new Point(targetX2, -halfHeight)
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
									Point2 = new Point(baseX, height - halfHeight / 4),
									Point3 = new Point(targetX, height)
								},
								new BezierSegment
								{
									Point1 = new Point(targetX, height),
									Point2 = new Point(targetX2, height + halfHeight / 4),
									Point3 = new Point(targetX2, height + halfHeight)
								},
							},
							IsClosed = false,
							IsFilled = false
						}
					}
				});
			}
		}

		var nodeBrush = GetBrush(_revisionRow.ColorId);
		
		drawingContext.DrawEllipse(nodeBrush, null, new Point(
			LeftMargin + _revisionRow.NodeIndex * NodeInterval, halfHeight), NodeSize, NodeSize);
	}
	
	private static ISolidColorBrush GetBrush(int colorId)
	{
		return _brushes[colorId % _brushes.Count];
	}
	
	private static Pen GetPen(int colorId)
	{
		return _pens[colorId % _brushes.Count];
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

	private RevisionGraphRow? _revisionRow;
}
