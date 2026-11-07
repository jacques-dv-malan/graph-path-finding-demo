namespace GraphPathFindingDemo.Core.Graph;

public sealed class GridGraph
{
    private readonly Dictionary<GridPoint, GridNode> _nodesByPoint;
    private readonly Dictionary<int, List<GridNode>> _adjacency;
    private readonly List<GridEdge> _edges;

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

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<GridNode> Nodes { get; }

    public IReadOnlyList<GridEdge> Edges { get; }

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

    public bool TryGetNode(GridPoint point, out GridNode node)
    {
        return _nodesByPoint.TryGetValue(point, out node!);
    }

    public bool IsConnected(GridNode a, GridNode b)
    {
        return _adjacency[a.Index].Contains(b);
    }

    public IReadOnlyList<GridNode> Neighbors(GridNode node)
    {
        return _adjacency[node.Index].AsReadOnly();
    }

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

    private bool Owns(GridNode node)
    {
        return _nodesByPoint.TryGetValue(node.Position, out var owner) && ReferenceEquals(owner, node);
    }

    private static bool AreOrthogonalNeighbours(GridNode a, GridNode b)
    {
        var rowDistance = Math.Abs(a.Row - b.Row);
        var columnDistance = Math.Abs(a.Column - b.Column);

        return (rowDistance == 1 && columnDistance == 0) || (rowDistance == 0 && columnDistance == 1);
    }
}
