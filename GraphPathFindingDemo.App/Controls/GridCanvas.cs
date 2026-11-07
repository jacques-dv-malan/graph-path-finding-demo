using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Graph;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphPathFindingDemo.App.Controls;

/// <summary>
/// Draws the grid, the search progress and the path found.
/// </summary>
/// <remarks>
/// The canvas holds no algorithm logic: it only turns a <see cref="RenderState"/> into shapes.
/// </remarks>
public sealed class GridCanvas : Control
{
    #region Constants
    /// <summary>
    /// Identifies the <see cref="RenderState"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RenderStateProperty = DependencyProperty.Register(
        nameof(RenderState),
        typeof(RenderState),
        typeof(GridCanvas),
        new PropertyMetadata(null, OnRenderStateChanged));

    /// <summary>
    /// The colour of the area behind the grid.
    /// </summary>
    public static readonly Brush BackgroundBrush = CreateBrush("#FF14181F");

    /// <summary>
    /// The colour of a cell that has been visited.
    /// </summary>
    public static readonly Brush VisitedBrush = CreateBrush("#FF2E4257");

    /// <summary>
    /// The colour of a cell waiting on the frontier.
    /// </summary>
    public static readonly Brush FrontierBrush = CreateBrush("#FFD79330");

    /// <summary>
    /// The colour of the walls.
    /// </summary>
    public static readonly Brush WallBrush = CreateBrush("#FF8A97A8");

    /// <summary>
    /// The colour of the path found.
    /// </summary>
    public static readonly Brush PathBrush = CreateBrush("#FF54D1DB");

    /// <summary>
    /// The colour of the start marker.
    /// </summary>
    public static readonly Brush StartBrush = CreateBrush("#FF3FBF50");

    /// <summary>
    /// The colour of the end marker.
    /// </summary>
    public static readonly Brush EndBrush = CreateBrush("#FFE03B3B");
    #endregion Constants

    #region Properties
    /// <summary>
    /// Gets or sets the state being drawn. Replacing the value repaints the canvas.
    /// </summary>
    public RenderState? RenderState
    {
        get => (RenderState?)GetValue(RenderStateProperty);
        set => SetValue(RenderStateProperty, value);
    }
    #endregion Properties

    #region Overridden Members
    /// <summary>
    /// Draws the grid, the search progress and the path found.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
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
    #endregion Overridden Members

    #region Private Methods
    /// <summary>
    /// Fills the cells that have been visited and the cells waiting on the frontier.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="state">The state being drawn.</param>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
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

    /// <summary>
    /// Strokes every cell edge that has no passage running along it.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
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

    /// <summary>
    /// Strokes the path found as a polyline through the centres of the cells it runs through.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="state">The state being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
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

    /// <summary>
    /// Draws the marker on the start node and the marker on the end node.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="state">The state being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
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

    /// <summary>
    /// Turns a list of nodes into a lookup of the node indexes it holds.
    /// </summary>
    /// <param name="nodes">The nodes to index.</param>
    /// <param name="count">The number of nodes in the graph.</param>
    /// <returns>A flag per node index, set for every node in <paramref name="nodes"/>.</returns>
    private static bool[] CreateFlags(IReadOnlyList<GridNode> nodes, int count)
    {
        var flags = new bool[count];

        foreach (var node in nodes)
        {
            flags[node.Index] = true;
        }

        return flags;
    }

    /// <summary>
    /// Returns the rectangle covered by a cell.
    /// </summary>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <param name="row">The row of the cell.</param>
    /// <param name="column">The column of the cell.</param>
    /// <returns>The rectangle of the cell.</returns>
    private static Rect CellRect(double offsetX, double offsetY, double cellSize, int row, int column)
    {
        return new Rect(offsetX + column * cellSize, offsetY + row * cellSize, cellSize, cellSize);
    }

    /// <summary>
    /// Returns the centre point of a cell.
    /// </summary>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <param name="row">The row of the cell.</param>
    /// <param name="column">The column of the cell.</param>
    /// <returns>The centre point of the cell.</returns>
    private static Point CellCentre(double offsetX, double offsetY, double cellSize, int row, int column)
    {
        return new Point(
            offsetX + (column + 0.5) * cellSize,
            offsetY + (row + 0.5) * cellSize);
    }

    /// <summary>
    /// Creates a frozen brush from a colour string.
    /// </summary>
    /// <param name="value">The colour to create the brush from.</param>
    /// <returns>The frozen brush.</returns>
    private static SolidColorBrush CreateBrush(string value)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        brush.Freeze();
        return brush;
    }
    #endregion Private Methods

    #region Events
    /// <summary>
    /// Repaints the canvas when the drawn state is replaced.
    /// </summary>
    /// <param name="d">The canvas whose state changed.</param>
    /// <param name="e">The event arguments of the change.</param>
    private static void OnRenderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((GridCanvas)d).InvalidateVisual();
    }
    #endregion Events
}
