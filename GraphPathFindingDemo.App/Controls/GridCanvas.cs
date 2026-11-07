using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.App.Controls;

public sealed class GridCanvas : Control
{
    public static readonly DependencyProperty RenderStateProperty = DependencyProperty.Register(
        nameof(RenderState),
        typeof(RenderState),
        typeof(GridCanvas),
        new PropertyMetadata(null, OnRenderStateChanged));

    public static readonly Brush BackgroundBrush = CreateBrush("#FF14181F");
    public static readonly Brush VisitedBrush = CreateBrush("#FF2E4257");
    public static readonly Brush FrontierBrush = CreateBrush("#FFD79330");
    public static readonly Brush WallBrush = CreateBrush("#FF8A97A8");
    public static readonly Brush PathBrush = CreateBrush("#FF54D1DB");
    public static readonly Brush StartBrush = CreateBrush("#FF3FBF50");
    public static readonly Brush EndBrush = CreateBrush("#FFE03B3B");

    public RenderState? RenderState
    {
        get => (RenderState?)GetValue(RenderStateProperty);
        set => SetValue(RenderStateProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var state = RenderState;

        if (state is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var graph = state.Scenario.Graph;
        var cellSize = Math.Min(ActualWidth / graph.Width, ActualHeight / graph.Height);

        if (cellSize <= 0)
        {
            return;
        }

        var offsetX = (ActualWidth - cellSize * graph.Width) / 2;
        var offsetY = (ActualHeight - cellSize * graph.Height) / 2;

        dc.DrawRectangle(
            BackgroundBrush,
            null,
            new Rect(offsetX, offsetY, cellSize * graph.Width, cellSize * graph.Height));

        DrawCells(dc, state, graph, offsetX, offsetY, cellSize);
        DrawWalls(dc, graph, offsetX, offsetY, cellSize);
        DrawPath(dc, state, offsetX, offsetY, cellSize);
        DrawMarkers(dc, state, offsetX, offsetY, cellSize);
    }

    private static void DrawCells(
        DrawingContext dc,
        RenderState state,
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var visited = CreateFlags(state.Visited, graph.Nodes.Count);
        var frontier = CreateFlags(state.Discovered, graph.Nodes.Count);

        for (var row = 0; row < graph.Height; row++)
        {
            for (var column = 0; column < graph.Width; column++)
            {
                var index = row * graph.Width + column;

                if (visited[index])
                {
                    dc.DrawRectangle(VisitedBrush, null, CellRect(offsetX, offsetY, cellSize, row, column));
                }

                if (frontier[index])
                {
                    dc.DrawRectangle(FrontierBrush, null, CellRect(offsetX, offsetY, cellSize, row, column));
                }
            }
        }
    }

    private static void DrawWalls(
        DrawingContext dc,
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var pen = new Pen(WallBrush, Math.Max(1.0, cellSize * 0.08));
        pen.Freeze();

        for (var row = 0; row < graph.Height; row++)
        {
            for (var column = 0; column < graph.Width; column++)
            {
                var cell = graph.NodeAt(new GridPoint(row, column));
                var left = offsetX + column * cellSize;
                var top = offsetY + row * cellSize;
                var right = left + cellSize;
                var bottom = top + cellSize;

                if (row == 0)
                {
                    dc.DrawLine(pen, new Point(left, top), new Point(right, top));
                }

                if (column == 0)
                {
                    dc.DrawLine(pen, new Point(left, top), new Point(left, bottom));
                }

                var hasRightPassage = column + 1 < graph.Width
                    && graph.IsConnected(cell, graph.NodeAt(new GridPoint(row, column + 1)));

                if (!hasRightPassage)
                {
                    dc.DrawLine(pen, new Point(right, top), new Point(right, bottom));
                }

                var hasBottomPassage = row + 1 < graph.Height
                    && graph.IsConnected(cell, graph.NodeAt(new GridPoint(row + 1, column)));

                if (!hasBottomPassage)
                {
                    dc.DrawLine(pen, new Point(left, bottom), new Point(right, bottom));
                }
            }
        }
    }

    private static void DrawPath(
        DrawingContext dc,
        RenderState state,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        if (state.Path.Count == 0)
        {
            return;
        }

        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            for (var index = 0; index < state.Path.Count; index++)
            {
                var node = state.Path[index];
                var centre = CellCentre(offsetX, offsetY, cellSize, node.Row, node.Column);

                if (index == 0)
                {
                    context.BeginFigure(centre, isFilled: false, isClosed: false);
                }
                else
                {
                    context.LineTo(centre, isStroked: true, isSmoothJoin: false);
                }
            }
        }

        geometry.Freeze();

        var pen = new Pen(PathBrush, Math.Max(2.0, cellSize * 0.16));
        pen.Freeze();

        dc.DrawGeometry(null, pen, geometry);
    }

    private static void DrawMarkers(
        DrawingContext dc,
        RenderState state,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var radius = cellSize * 0.3;

        var startCentre = CellCentre(offsetX, offsetY, cellSize, state.Scenario.Start.Row, state.Scenario.Start.Column);
        dc.DrawEllipse(StartBrush, null, startCentre, radius, radius);

        var endCentre = CellCentre(offsetX, offsetY, cellSize, state.Scenario.End.Row, state.Scenario.End.Column);
        dc.DrawEllipse(EndBrush, null, endCentre, radius, radius);
    }

    private static bool[] CreateFlags(IReadOnlyList<GridNode> nodes, int count)
    {
        var flags = new bool[count];

        foreach (var node in nodes)
        {
            flags[node.Index] = true;
        }

        return flags;
    }

    private static Rect CellRect(double offsetX, double offsetY, double cellSize, int row, int column)
    {
        return new Rect(offsetX + column * cellSize, offsetY + row * cellSize, cellSize, cellSize);
    }

    private static Point CellCentre(double offsetX, double offsetY, double cellSize, int row, int column)
    {
        return new Point(
            offsetX + (column + 0.5) * cellSize,
            offsetY + (row + 0.5) * cellSize);
    }

    private static SolidColorBrush CreateBrush(string value)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        brush.Freeze();
        return brush;
    }

    private static void OnRenderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((GridCanvas)d).InvalidateVisual();
    }
}
