using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using GitAlpha.Git;

namespace GitAlpha.Avalonia.Controls;

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
			_renderGeometryResolved = false;
			
			if (_revisionRow is not null)
			{
				_revisionRow.BindControl = null;
			}
			
			_revisionRow = value;

			if (_revisionRow is not null)
			{
				_revisionRow.BindControl = this;
				Width = LeftMargin + _revisionRow.AllNodes * NodeInterval;
			}
		}
	}

	public static readonly DirectProperty<GraphRowControl, RevisionGraphRow?> GraphRowProperty =
		AvaloniaProperty.RegisterDirect<GraphRowControl, RevisionGraphRow?>(
			nameof(RevisionRow),
			o => o.GraphRow,
			(o, v) => o.GraphRow = v);

	public int LeftMargin { get; set; } = 8;
	
	public int NodeInterval { get; set; } = 16;
	
	public double NodeSize { get; set; } = 5;

	public double RenderHeight
	{
		get
		{
			ResolveRenderGeometryWithParent();
			return _renderHeight;
		}
	}

	private bool _renderGeometryResolved = false;

	private double _renderHeight;
	private double _renderYShift;
	
	private void ResolveRenderGeometryWithParent()
	{
		if(_renderGeometryResolved)
			return;
		
		var panel = (DockPanel)Parent!;
		var listBoxItem = (ListBoxItem)panel.Parent!;

		_renderYShift = - (panel.Margin.Bottom + listBoxItem.Margin.Bottom + listBoxItem.Padding.Bottom);
		_renderHeight = listBoxItem.Bounds.Height;

		_renderGeometryResolved = true;
	}

	protected override void OnSizeChanged(SizeChangedEventArgs e)
	{
		_renderGeometryResolved = false;
		_revisionRow?.Up?.BindControl?.InvalidateVisual();
		_revisionRow?.Down?.BindControl?.InvalidateVisual();
		base.OnSizeChanged(e);
	}

	public override void Render(DrawingContext drawingContext)
	{
		if (_revisionRow is null)
			return;
		
		ResolveRenderGeometryWithParent();
		var yShift = _renderYShift;
		var height = _renderHeight;

		var upHeight = _revisionRow.Up?.BindControl?.RenderHeight ?? height;
		var downHeight = _revisionRow.Down?.BindControl?.RenderHeight ?? height;
		
		foreach (var conn in _revisionRow.ConnectionsRender)
		{
			var pen = GetPen(conn.ColorId);
			var baseX = LeftMargin + NodeInterval * conn.Index;
			var targetX = baseX + conn.Delta * NodeInterval / 2;
			var targetX2 = baseX + conn.Delta * NodeInterval;
			
			if (conn.Up)
			{
				var middleHeight = Math.Min(height, upHeight) / 2 / 4;
				
				drawingContext.DrawGeometry(null, pen, new PathGeometry
				{
					Figures = new PathFigures
					{
						new PathFigure
						{
							StartPoint = new Point(baseX, height / 2 + yShift),
							Segments = new PathSegments()
							{
								new BezierSegment
								{
									Point1 = new Point(baseX, height / 2 + yShift),
									Point2 = new Point(baseX, middleHeight + yShift),
									Point3 = new Point(targetX, 0 + yShift)
								},
								new BezierSegment
								{
									Point1 = new Point(targetX, 0 + yShift),
									Point2 = new Point(targetX2, - middleHeight + yShift),
									Point3 = new Point(targetX2, - upHeight / 2 + yShift)
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
				var middleHeight = Math.Min(height, downHeight) / 2 / 4;
				
				drawingContext.DrawGeometry(null, pen, new PathGeometry
				{
					Figures = new PathFigures
					{
						new PathFigure
						{
							StartPoint = new Point(baseX, height / 2 + yShift),
							Segments = new PathSegments()
							{
								new BezierSegment
								{
									Point1 = new Point(baseX, height / 2 + yShift),
									Point2 = new Point(baseX, height - middleHeight + yShift),
									Point3 = new Point(targetX, height + yShift)
								},
								new BezierSegment
								{
									Point1 = new Point(targetX, height + yShift),
									Point2 = new Point(targetX2, height + middleHeight + yShift),
									Point3 = new Point(targetX2, height + downHeight / 2 + yShift)
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
			LeftMargin + _revisionRow.NodeIndex * NodeInterval, height / 2 + yShift), NodeSize, NodeSize);
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
		_colorPreset.Select(color => (ISolidColorBrush)new ImmutableSolidColorBrush(color, 1d)).ToList();

	private static readonly IList<Pen> _pens =
		_brushes.Select(brush => new Pen(brush, 2, null, PenLineCap.Flat)).ToList();

	private RevisionGraphRow? _revisionRow;
}
