namespace GraphPathFindingDemo.Core.Graph;

/// <summary>
/// A rectangular grid of <see cref="GridNode"/> instances joined by <see cref="GridEdge"/>
/// passages. Every node is created up front, row by row, and passages are added afterwards by
/// layout generators.
/// </summary>
/// <remarks>
/// Walls are not modelled separately: a wall is simply the absence of an edge between two
/// orthogonal cells.
/// </remarks>
public sealed class GridGraph
{
    #region Instance Fields
    private readonly Dictionary<GridPoint, GridNode> _nodesByPoint;
    private readonly Dictionary<int, List<GridNode>> _adjacency;
    private readonly List<GridEdge> _edges;
    #endregion Instance Fields

    #region Properties
    /// <summary>
    /// Gets the number of columns in the grid.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the number of rows in the grid.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets every node of the grid, ordered row by row.
    /// </summary>
    public IReadOnlyList<GridNode> Nodes { get; }

    /// <summary>
    /// Gets one entry per passage, in the order the passages were created.
    /// </summary>
    public IReadOnlyList<GridEdge> Edges { get; }
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="GridGraph"/> class and creates every node.
    /// </summary>
    /// <param name="width">The number of columns. Must be at least <c>1</c>.</param>
    /// <param name="height">The number of rows. Must be at least <c>1</c>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is less than <c>1</c>.
    /// </exception>
    public GridGraph(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        Width = width;
        Height = height;

        var nodes = new List<GridNode>(width * height);
        _nodesByPoint = new Dictionary<GridPoint, GridNode>(width * height);
        _adjacency = new Dictionary<int, List<GridNode>>(width * height);
        _edges = new List<GridEdge>();

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var position = new GridPoint(row, column);
                var node = new GridNode(row * width + column, position);

                nodes.Add(node);
                _nodesByPoint.Add(position, node);
                _adjacency.Add(node.Index, []);
            }
        }

        Nodes = nodes.AsReadOnly();
        Edges = _edges.AsReadOnly();
    }
    #endregion Constructor

    #region Public Methods
    /// <summary>
    /// Returns the node at the given coordinate.
    /// </summary>
    /// <param name="point">The coordinate to resolve.</param>
    /// <returns>The node sitting at <paramref name="point"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="point"/> lies outside the grid.
    /// </exception>
    public GridNode NodeAt(GridPoint point)
    {
        if (!TryGetNode(point, out var node))
        {
            throw new ArgumentOutOfRangeException(
                nameof(point),
                point,
                $"The point ({point.Row}, {point.Column}) is outside the {Width}x{Height} grid.");
        }

        return node;
    }

    /// <summary>
    /// Attempts to return the node at the given coordinate.
    /// </summary>
    /// <param name="point">The coordinate to resolve.</param>
    /// <param name="node">
    /// When this method returns, the node at <paramref name="point"/>, or <see langword="null"/>
    /// when the coordinate lies outside the grid.
    /// </param>
    /// <returns><see langword="true"/> when the coordinate is inside the grid.</returns>
    public bool TryGetNode(GridPoint point, out GridNode node)
    {
        return _nodesByPoint.TryGetValue(point, out node!);
    }

    /// <summary>
    /// Determines whether a passage runs directly between two nodes.
    /// </summary>
    /// <param name="a">The first node.</param>
    /// <param name="b">The second node.</param>
    /// <returns><see langword="true"/> when the two nodes are joined by an edge.</returns>
    public bool IsConnected(GridNode a, GridNode b)
    {
        return _adjacency[a.Index].Contains(b);
    }

    /// <summary>
    /// Returns the nodes reachable from the given node in a single step.
    /// </summary>
    /// <param name="node">The node whose neighbours are requested.</param>
    /// <returns>The adjacent nodes of <paramref name="node"/>.</returns>
    public IReadOnlyList<GridNode> Neighbors(GridNode node)
    {
        return _adjacency[node.Index].AsReadOnly();
    }
    #endregion Public Methods

    #region Internal Methods
    /// <summary>
    /// Adds a passage between two orthogonal nodes. Used by layout generators to build the graph.
    /// </summary>
    /// <param name="a">The first node of the passage.</param>
    /// <param name="b">The second node of the passage.</param>
    /// <exception cref="ArgumentException">
    /// A node does not belong to this graph, or the two nodes are not orthogonal neighbours.
    /// </exception>
    /// <exception cref="InvalidOperationException">The two nodes are already connected.</exception>
    internal void Connect(GridNode a, GridNode b)
    {
        if (!Owns(a) || !Owns(b))
        {
            throw new ArgumentException("Both nodes must belong to this graph.", nameof(a));
        }

        if (!AreOrthogonalNeighbours(a, b))
        {
            throw new ArgumentException(
                $"Nodes ({a.Row}, {a.Column}) and ({b.Row}, {b.Column}) are not orthogonal neighbours.",
                nameof(a));
        }

        if (IsConnected(a, b))
        {
            throw new InvalidOperationException(
                $"Nodes ({a.Row}, {a.Column}) and ({b.Row}, {b.Column}) are already connected.");
        }

        _adjacency[a.Index].Add(b);
        _adjacency[b.Index].Add(a);
        _edges.Add(new GridEdge(a, b));
    }
    #endregion Internal Methods

    #region Private Methods
    /// <summary>
    /// Determines whether the given node is one of this graph's own nodes.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns><see langword="true"/> when the node belongs to this graph.</returns>
    private bool Owns(GridNode node)
    {
        return _nodesByPoint.TryGetValue(node.Position, out var owner) && ReferenceEquals(owner, node);
    }

    /// <summary>
    /// Determines whether two nodes sit directly above, below, left or right of each other.
    /// </summary>
    /// <param name="a">The first node.</param>
    /// <param name="b">The second node.</param>
    /// <returns><see langword="true"/> when the nodes are orthogonal neighbours.</returns>
    private static bool AreOrthogonalNeighbours(GridNode a, GridNode b)
    {
        var rowDistance = Math.Abs(a.Row - b.Row);
        var columnDistance = Math.Abs(a.Column - b.Column);

        return (rowDistance == 1 && columnDistance == 0) || (rowDistance == 0 && columnDistance == 1);
    }
    #endregion Private Methods
}
