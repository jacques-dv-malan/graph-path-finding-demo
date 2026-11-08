using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Graph;
using GraphPathFindingDemo.Core.Terrain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    /// The colour of the lowest ground the terrain reaches.
    /// </summary>
    public static readonly Color TerrainLowColor = Color.FromRgb(0x1A, 0x24, 0x36);

    /// <summary>
    /// The colour of the highest ground the terrain reaches.
    /// </summary>
    public static readonly Color TerrainHighColor = Color.FromRgb(0x8F, 0xA3, 0xB5);

    /// <summary>
    /// The colour of every height the terrain can reach, from the lowest to the highest.
    /// </summary>
    private static readonly Brush[] TerrainBrushes = BuildTerrainBrushes();

    /// <summary>
    /// The colour of a cell that has been visited. It is partly transparent, so that the
    /// ground underneath it still shows.
    /// </summary>
    public static readonly Brush VisitedBrush = CreateBrush("#B22E6DB4");

    /// <summary>
    /// The colour of a cell waiting on the frontier.
    /// </summary>
    public static readonly Brush FrontierBrush = CreateBrush("#FFD79330");

    /// <summary>
    /// The colour of the walls.
    /// </summary>
    public static readonly Brush WallBrush = CreateBrush("#FFB4C0D0");

    /// <summary>
    /// The colour of the path found.
    /// </summary>
    public static readonly Brush PathBrush = CreateBrush("#FF5FE3F0");

    /// <summary>
    /// The colour of the start marker.
    /// </summary>
    public static readonly Brush StartBrush = CreateBrush("#FF45E06B");

    /// <summary>
    /// The colour of the end marker.
    /// </summary>
    public static readonly Brush EndBrush = CreateBrush("#FFFF4D5E");
    #endregion Constants

    #region Instance Fields
    private TerrainLayer? _terrain;
    private WallLayer? _walls;
    private bool[] _visitedFlags = [];
    private bool[] _frontierFlags = [];
    #endregion Instance Fields

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

        var offsetX = (ActualWidth - (cellSize * graph.Width)) / 2;
        var offsetY = (ActualHeight - (cellSize * graph.Height)) / 2;

        DrawTerrain(dc, graph, offsetX, offsetY, cellSize);
        DrawCells(dc, state, graph, offsetX, offsetY, cellSize);
        DrawWalls(dc, graph, offsetX, offsetY, cellSize);
        DrawPath(dc, state, offsetX, offsetY, cellSize);
        DrawMarkers(dc, state, offsetX, offsetY, cellSize);
    }

    /// <summary>
    /// Reports the cell the user clicked on with the left button, which moves the start.
    /// </summary>
    /// <param name="e">The arguments of the click.</param>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        ReportClick(e, EndpointTarget.Start);
    }

    /// <summary>
    /// Reports the cell the user clicked on with the right button, which moves the end.
    /// </summary>
    /// <param name="e">The arguments of the click.</param>
    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);

        ReportClick(e, EndpointTarget.End);
    }
    #endregion Overridden Members

    #region Internal Classes
    /// <summary>
    /// The ground of a grid, built once and reused for as long as the grid and the size of a
    /// cell stay the same.
    /// </summary>
    private sealed class TerrainLayer
    {
        /// <summary>
        /// Gets the graph the ground belongs to.
        /// </summary>
        public GridGraph Graph { get; }

        /// <summary>
        /// Gets the geometry of every height of the ground, from the lowest to the highest.
        /// </summary>
        public StreamGeometry[] Levels { get; }

        /// <summary>
        /// Gets the size of a single cell the ground was built for.
        /// </summary>
        public double CellSize { get; }

        /// <summary>
        /// Gets the left edge of the grid the ground was built for.
        /// </summary>
        public double OffsetX { get; }

        /// <summary>
        /// Gets the top edge of the grid the ground was built for.
        /// </summary>
        public double OffsetY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainLayer"/> class.
        /// </summary>
        /// <param name="graph">The graph the ground belongs to.</param>
        /// <param name="levels">The geometry of every height of the ground.</param>
        /// <param name="cellSize">The size of a single cell.</param>
        /// <param name="offsetX">The left edge of the grid.</param>
        /// <param name="offsetY">The top edge of the grid.</param>
        public TerrainLayer(
            GridGraph graph,
            StreamGeometry[] levels,
            double cellSize,
            double offsetX,
            double offsetY)
        {
            Graph = graph;
            Levels = levels;
            CellSize = cellSize;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }

    /// <summary>
    /// The walls of a grid, built once and reused for as long as the grid and the size of a cell
    /// stay the same.
    /// </summary>
    private sealed class WallLayer
    {
        /// <summary>
        /// Gets the graph the walls belong to.
        /// </summary>
        public GridGraph Graph { get; }

        /// <summary>
        /// Gets the geometry of the walls.
        /// </summary>
        public StreamGeometry Geometry { get; }

        /// <summary>
        /// Gets the pen the walls are stroked with.
        /// </summary>
        public Pen Pen { get; }

        /// <summary>
        /// Gets the size of a single cell the walls were built for.
        /// </summary>
        public double CellSize { get; }

        /// <summary>
        /// Gets the left edge of the grid the walls were built for.
        /// </summary>
        public double OffsetX { get; }

        /// <summary>
        /// Gets the top edge of the grid the walls were built for.
        /// </summary>
        public double OffsetY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WallLayer"/> class.
        /// </summary>
        /// <param name="graph">The graph the walls belong to.</param>
        /// <param name="geometry">The geometry of the walls.</param>
        /// <param name="pen">The pen the walls are stroked with.</param>
        /// <param name="cellSize">The size of a single cell.</param>
        /// <param name="offsetX">The left edge of the grid.</param>
        /// <param name="offsetY">The top edge of the grid.</param>
        public WallLayer(
            GridGraph graph,
            StreamGeometry geometry,
            Pen pen,
            double cellSize,
            double offsetX,
            double offsetY)
        {
            Graph = graph;
            Geometry = geometry;
            Pen = pen;
            CellSize = cellSize;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
    #endregion Internal Classes

    #region Private Methods
    /// <summary>
    /// Reports the cell a click landed on.
    /// </summary>
    /// <param name="e">The arguments of the click.</param>
    /// <param name="target">The endpoint the click moves.</param>
    private void ReportClick(MouseButtonEventArgs e, EndpointTarget target)
    {
        if (TryGetCellAt(e.GetPosition(this), out var point))
        {
            CellClicked?.Invoke(this, new CellClickedEventArgs(point, target));
        }
    }

    /// <summary>
    /// Works out which cell of the grid a point lies on.
    /// </summary>
    /// <param name="position">The point to place.</param>
    /// <param name="point">The cell the point lies on.</param>
    /// <returns><see langword="true"/> when the point lies on a cell of the grid.</returns>
    private bool TryGetCellAt(Point position, out GridPoint point)
    {
        point = default;

        var state = RenderState;

        if (state is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return false;
        }

        var graph = state.Scenario.Graph;
        var cellSize = Math.Min(ActualWidth / graph.Width, ActualHeight / graph.Height);

        if (cellSize <= 0)
        {
            return false;
        }

        var offsetX = (ActualWidth - (cellSize * graph.Width)) / 2;
        var offsetY = (ActualHeight - (cellSize * graph.Height)) / 2;

        var column = (int)Math.Floor((position.X - offsetX) / cellSize);
        var row = (int)Math.Floor((position.Y - offsetY) / cellSize);

        if (row < 0 || column < 0 || row >= graph.Height || column >= graph.Width)
        {
            return false;
        }

        point = new GridPoint(row, column);
        return true;
    }

    /// <summary>
    /// Fills the cells with the colour that belongs to the height of the ground they stand on.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    private void DrawTerrain(
        DrawingContext dc,
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var terrain = GetTerrain(graph, offsetX, offsetY, cellSize);

        for (var level = 0; level < terrain.Levels.Length; level++)
        {
            dc.DrawGeometry(TerrainBrushes[level], null, terrain.Levels[level]);
        }
    }

    /// <summary>
    /// Returns the cached ground of the grid, building it when the grid or the size of a cell
    /// changed.
    /// </summary>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <returns>The ground to draw.</returns>
    private TerrainLayer GetTerrain(GridGraph graph, double offsetX, double offsetY, double cellSize)
    {
        var cached = _terrain;

        if (cached is not null
            && ReferenceEquals(cached.Graph, graph)
            && cached.CellSize == cellSize
            && cached.OffsetX == offsetX
            && cached.OffsetY == offsetY)
        {
            return cached;
        }

        var layer = new TerrainLayer(
            graph,
            BuildTerrainGeometries(graph, offsetX, offsetY, cellSize),
            cellSize,
            offsetX,
            offsetY);

        _terrain = layer;
        return layer;
    }

    /// <summary>
    /// Builds the geometry of the ground of every height the terrain can reach.
    /// </summary>
    /// <param name="graph">The graph the ground belongs to.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <returns>The frozen geometry of every height, from the lowest to the highest.</returns>
    private static StreamGeometry[] BuildTerrainGeometries(
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var heights = HeightField.MaxHeight + 1;
        var geometries = new StreamGeometry[heights];

        for (var height = 0; height < heights; height++)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                for (var row = 0; row < graph.Height; row++)
                {
                    var column = 0;

                    while (column < graph.Width)
                    {
                        if (HeightOf(graph, row, column) != height)
                        {
                            column++;
                            continue;
                        }

                        var start = column;

                        while (column < graph.Width && HeightOf(graph, row, column) == height)
                        {
                            column++;
                        }

                        AddRun(context, offsetX, offsetY, cellSize, row, start, column);
                    }
                }
            }

            geometry.Freeze();
            geometries[height] = geometry;
        }

        return geometries;
    }

    /// <summary>
    /// Returns the height of the ground at a place, held within the range the terrain reaches.
    /// </summary>
    /// <param name="graph">The graph the ground belongs to.</param>
    /// <param name="row">The row of the cell.</param>
    /// <param name="column">The column of the cell.</param>
    /// <returns>The height of the ground.</returns>
    private static int HeightOf(GridGraph graph, int row, int column)
    {
        var height = graph.Nodes[(row * graph.Width) + column].Height;

        return Math.Clamp(height, 0, HeightField.MaxHeight);
    }

    /// <summary>
    /// Builds one brush for every height the terrain can reach.
    /// </summary>
    /// <returns>The brush of every height, from the lowest to the highest.</returns>
    private static Brush[] BuildTerrainBrushes()
    {
        var brushes = new Brush[HeightField.MaxHeight + 1];

        for (var height = 0; height < brushes.Length; height++)
        {
            var amount = height / (double)HeightField.MaxHeight;

            var colour = Color.FromRgb(
                (byte)(TerrainLowColor.R + ((TerrainHighColor.R - TerrainLowColor.R) * amount)),
                (byte)(TerrainLowColor.G + ((TerrainHighColor.G - TerrainLowColor.G) * amount)),
                (byte)(TerrainLowColor.B + ((TerrainHighColor.B - TerrainLowColor.B) * amount)));

            var brush = new SolidColorBrush(colour);
            brush.Freeze();

            brushes[height] = brush;
        }

        return brushes;
    }

    /// <summary>
    /// Fills the cells that have been visited and the cells waiting on the frontier.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="state">The state being drawn.</param>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    private void DrawCells(
        DrawingContext dc,
        RenderState state,
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var visited = CreateFlags(_visitedFlags, state.Visited, graph.Nodes.Count);
        var frontier = CreateFlags(_frontierFlags, state.Discovered, graph.Nodes.Count);

        _visitedFlags = visited;
        _frontierFlags = frontier;

        var visitedRuns = BuildRuns(visited, graph.Width, graph.Height, offsetX, offsetY, cellSize);
        var frontierRuns = BuildRuns(frontier, graph.Width, graph.Height, offsetX, offsetY, cellSize);

        dc.DrawGeometry(VisitedBrush, null, visitedRuns);
        dc.DrawGeometry(FrontierBrush, null, frontierRuns);
    }

    /// <summary>
    /// Builds the geometry of the cells that are flagged, merging the cells that sit next to
    /// each other into one rectangle.
    /// </summary>
    /// <param name="flags">The flag of every cell of the grid.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <returns>The frozen geometry of the flagged cells.</returns>
    private static StreamGeometry BuildRuns(
        bool[] flags,
        int width,
        int height,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            for (var row = 0; row < height; row++)
            {
                var column = 0;

                while (column < width)
                {
                    if (!flags[(row * width) + column])
                    {
                        column++;
                        continue;
                    }

                    var start = column;

                    while (column < width && flags[(row * width) + column])
                    {
                        column++;
                    }

                    AddRun(context, offsetX, offsetY, cellSize, row, start, column);
                }
            }
        }

        geometry.Freeze();
        return geometry;
    }

    /// <summary>
    /// Adds a run of cells to the geometry being built, as one rectangle.
    /// </summary>
    /// <param name="context">The context of the geometry being built.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <param name="row">The row the run sits in.</param>
    /// <param name="startColumn">The first column of the run.</param>
    /// <param name="endColumn">The column just past the run.</param>
    private static void AddRun(
        StreamGeometryContext context,
        double offsetX,
        double offsetY,
        double cellSize,
        int row,
        int startColumn,
        int endColumn)
    {
        var left = offsetX + (startColumn * cellSize);
        var right = offsetX + (endColumn * cellSize);
        var top = offsetY + (row * cellSize);
        var bottom = top + cellSize;

        context.BeginFigure(new Point(left, top), isFilled: true, isClosed: true);
        context.LineTo(new Point(right, top), isStroked: false, isSmoothJoin: false);
        context.LineTo(new Point(right, bottom), isStroked: false, isSmoothJoin: false);
        context.LineTo(new Point(left, bottom), isStroked: false, isSmoothJoin: false);
    }

    /// <summary>
    /// Strokes every cell edge that has no passage running along it.
    /// </summary>
    /// <param name="dc">The drawing context to draw into.</param>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    private void DrawWalls(
        DrawingContext dc,
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var walls = GetWalls(graph, offsetX, offsetY, cellSize);

        dc.DrawGeometry(null, walls.Pen, walls.Geometry);
    }

    /// <summary>
    /// Returns the cached wall layer, building it when the grid or the size of a cell changed.
    /// </summary>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <returns>The wall layer to draw.</returns>
    private WallLayer GetWalls(GridGraph graph, double offsetX, double offsetY, double cellSize)
    {
        var cached = _walls;

        if (cached is not null
            && ReferenceEquals(cached.Graph, graph)
            && cached.CellSize == cellSize
            && cached.OffsetX == offsetX
            && cached.OffsetY == offsetY)
        {
            return cached;
        }

        var layer = new WallLayer(
            graph,
            BuildWallGeometry(graph, offsetX, offsetY, cellSize),
            BuildWallPen(cellSize),
            cellSize,
            offsetX,
            offsetY);

        _walls = layer;
        return layer;
    }

    /// <summary>
    /// Builds the geometry of every wall of the grid.
    /// </summary>
    /// <param name="graph">The graph being drawn.</param>
    /// <param name="offsetX">The left edge of the grid.</param>
    /// <param name="offsetY">The top edge of the grid.</param>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <returns>The frozen geometry of the walls.</returns>
    private static StreamGeometry BuildWallGeometry(
        GridGraph graph,
        double offsetX,
        double offsetY,
        double cellSize)
    {
        var nodeCount = graph.Nodes.Count;
        var passages = new HashSet<int>();

        foreach (var edge in graph.Edges)
        {
            var from = Math.Min(edge.From.Index, edge.To.Index);
            var to = Math.Max(edge.From.Index, edge.To.Index);

            passages.Add((from * nodeCount) + to);
        }

        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            for (var row = 0; row < graph.Height; row++)
            {
                for (var column = 0; column < graph.Width; column++)
                {
                    var index = (row * graph.Width) + column;
                    var left = offsetX + (column * cellSize);
                    var top = offsetY + (row * cellSize);
                    var right = left + cellSize;
                    var bottom = top + cellSize;

                    if (row == 0)
                    {
                        DrawWall(context, left, top, right, top);
                    }

                    if (column == 0)
                    {
                        DrawWall(context, left, top, left, bottom);
                    }

                    if (column + 1 == graph.Width || !passages.Contains((index * nodeCount) + index + 1))
                    {
                        DrawWall(context, right, top, right, bottom);
                    }

                    if (row + 1 == graph.Height || !passages.Contains((index * nodeCount) + index + graph.Width))
                    {
                        DrawWall(context, left, bottom, right, bottom);
                    }
                }
            }
        }

        geometry.Freeze();
        return geometry;
    }

    /// <summary>
    /// Adds a single wall to the geometry being built.
    /// </summary>
    /// <param name="context">The context of the geometry being built.</param>
    /// <param name="x1">The horizontal position of the start of the wall.</param>
    /// <param name="y1">The vertical position of the start of the wall.</param>
    /// <param name="x2">The horizontal position of the end of the wall.</param>
    /// <param name="y2">The vertical position of the end of the wall.</param>
    private static void DrawWall(StreamGeometryContext context, double x1, double y1, double x2, double y2)
    {
        context.BeginFigure(new Point(x1, y1), isFilled: false, isClosed: false);
        context.LineTo(new Point(x2, y2), isStroked: true, isSmoothJoin: false);
    }

    /// <summary>
    /// Creates the pen the walls are stroked with.
    /// </summary>
    /// <param name="cellSize">The size of a single cell.</param>
    /// <returns>The frozen pen.</returns>
    private static Pen BuildWallPen(double cellSize)
    {
        var pen = new Pen(WallBrush, Math.Max(1.0, cellSize * 0.08));
        pen.Freeze();

        return pen;
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
    private static bool[] CreateFlags(bool[] flags, IReadOnlyCollection<GridNode> nodes, int count)
    {
        if (flags.Length != count)
        {
            flags = new bool[count];
        }
        else
        {
            Array.Clear(flags, 0, count);
        }

        foreach (var node in nodes)
        {
            flags[node.Index] = true;
        }

        return flags;
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
    /// Raised when the user clicks a cell of the grid.
    /// </summary>
    public event EventHandler<CellClickedEventArgs>? CellClicked;

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
