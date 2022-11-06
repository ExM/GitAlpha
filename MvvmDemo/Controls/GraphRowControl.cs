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
	
	public int NodeInterval { get; set; } = 10;
	
	public double NodeSize { get; set; } = 5;

	public override void Render(DrawingContext drawingContext)
	{
		if (_revisionRow is null)
			return;

		var halfHeight = Bounds.Height / 2;
		var transitUp = halfHeight - NodeSize / 2 - 4 ;
		var transitDown = halfHeight + NodeSize / 2 + 4;

		var offset = LeftMargin;

		foreach (var id in _revisionRow.Render)
		{
			var brush = GetBrush(id);

			if (id == _revisionRow.Id)
			{
				drawingContext.DrawEllipse(brush, null, new Point(offset, halfHeight), NodeSize, NodeSize);
			}
			else
			{
				drawingContext.DrawLine(new Pen(brush, 2), new Point(offset, transitUp), new Point(offset, transitDown));
			}
			offset += NodeInterval;
		}

		foreach (var mergeTransit in _revisionRow.MergeTransitRender)
		{
			var brush = GetBrush(mergeTransit.ParentId);
			var transitX = NodeInterval * mergeTransit.TransitIndex + LeftMargin;
			var nodeX = NodeInterval * mergeTransit.NodeIndex + LeftMargin;
			
			if (mergeTransit.TransitIndex < mergeTransit.NodeIndex)
			{ // left merge
				drawingContext.DrawLine(new Pen(brush, 2), new Point(transitX, transitDown), new Point(transitX + NodeInterval/2, halfHeight));
				drawingContext.DrawLine(new Pen(brush, 2), new Point(transitX + NodeInterval/2, halfHeight), new Point(nodeX - NodeSize - 1, halfHeight));
			}
			else
			{  // right merge
				drawingContext.DrawLine(new Pen(brush, 2), new Point(transitX, transitDown), new Point(transitX - NodeInterval/2, halfHeight));
				drawingContext.DrawLine(new Pen(brush, 2), new Point(transitX - NodeInterval/2, halfHeight), new Point(nodeX + NodeSize + 1, halfHeight));
			}
		}

		foreach (var conn in _revisionRow.ConnectionsRender)
		{
			var brush = GetBrush(conn.ConnId);
			var baseX = LeftMargin + NodeInterval * conn.Index;
			var targetX = baseX + conn.Delta * NodeInterval / 2;
			
			if (conn.Up)
			{
				drawingContext.DrawLine(new Pen(brush, 2), new Point(baseX, transitUp), new Point(targetX, 0));
			}
			else
			{
				drawingContext.DrawLine(new Pen(brush, 2), new Point(baseX, transitDown), new Point(targetX, Bounds.Height));
			}
		}
	}

	private static ISolidColorBrush GetBrush(ObjectId id)
	{
		return _brushes[Math.Abs(id.GetHashCode()) % _brushes.Count];
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


	private RevisionRow? _revisionRow;
}
