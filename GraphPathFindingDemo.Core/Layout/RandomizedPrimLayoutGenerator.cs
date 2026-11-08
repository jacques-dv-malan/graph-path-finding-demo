using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Generates a maze by growing one connected region outwards, taking the passages that leave
/// the region at random.
/// </summary>
/// <remarks>
/// The region starts at one cell and swallows one neighbour at a time, picked at random from the
/// passages leading out of it. The result is a perfect maze: a spanning tree with
/// <c>Edges.Count == Nodes.Count - 1</c> in which every cell is reachable from every other
/// cell. The passages are short and the region spreads wide, so the maze looks blobby rather
/// than corridor-like.
/// </remarks>
public sealed class RandomizedPrimLayoutGenerator : ILayoutGenerator
{
    #region Constants
    /// <summary>
    /// The directions a cell may be grown in.
    /// </summary>
    private static readonly GridPoint[] Directions =
    [
        new GridPoint(-1, 0),
        new GridPoint(0, 1),
        new GridPoint(1, 0),
        new GridPoint(0, -1)
    ];
    #endregion Constants

    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.RandomizedPrim;
    #endregion Properties

    #region Internal Classes
    /// <summary>
    /// A passage that may be taken out of the region grown so far.
    /// </summary>
    /// <param name="From">The cell the passage starts at. It belongs to the region already
    /// grown.</param>
    /// <param name="To">The cell the passage ends at. It does not belong to the region yet.</param>
    private readonly record struct Passage(GridNode From, GridNode To);
    #endregion Internal Classes

    #region Public Methods
    /// <summary>
    /// Generates a maze for the requested grid.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout. The same seed always
    /// produces the same maze.</param>
    /// <returns>The generated graph.</returns>
    public GridGraph Generate(int width, int height, int seed)
    {
        var graph = new GridGraph(width, height);
        var random = new Random(seed);
        var visited = new bool[height, width];
        var frontier = new List<Passage>();

        var start = graph.NodeAt(new GridPoint(random.Next(height), random.Next(width)));
        visited[start.Row, start.Column] = true;
        AddPassages(graph, frontier, start, visited);

        while (frontier.Count > 0)
        {
            var index = random.Next(frontier.Count);
            var passage = frontier[index];

            // The passage is taken out of the list without keeping the order of the rest.
            frontier[index] = frontier[^1];
            frontier.RemoveAt(frontier.Count - 1);

            // A cell can be reached by more than one passage, but it is only grown once.
            if (visited[passage.To.Row, passage.To.Column])
            {
                continue;
            }

            visited[passage.To.Row, passage.To.Column] = true;
            graph.Connect(passage.From, passage.To);
            AddPassages(graph, frontier, passage.To, visited);
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Adds a passage to every neighbour of the given cell that has not been visited yet.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="frontier">The passages leading out of the region grown so far.</param>
    /// <param name="node">The cell the passages start at.</param>
    /// <param name="visited">The cells that already belong to the region.</param>
    private static void AddPassages(
        GridGraph graph,
        List<Passage> frontier,
        GridNode node,
        bool[,] visited)
    {
        foreach (var direction in Directions)
        {
            var row = node.Row + direction.Row;
            var column = node.Column + direction.Column;

            if (row < 0 || row >= graph.Height || column < 0 || column >= graph.Width)
            {
                continue;
            }

            if (visited[row, column])
            {
                continue;
            }

            frontier.Add(new Passage(node, graph.NodeAt(new GridPoint(row, column))));
        }
    }
    #endregion Private Methods
}
