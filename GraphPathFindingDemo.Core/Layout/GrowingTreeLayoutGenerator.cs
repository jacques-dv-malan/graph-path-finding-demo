using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Generates a maze by growing a tree outwards, taking the cell to grow from out of the whole
/// set of cells reached so far.
/// </summary>
/// <remarks>
/// Every cell that is reached joins the set of cells the tree may grow from, and one of them is
/// picked at random to grow from. A cell leaves the set once it has no cell left to reach. The
/// tree therefore grows from many fronts at once, which makes it bushier than the recursive
/// backtracker, whose passages run deep into the grid. Every cell is reached by a single
/// passage, so the result is a perfect maze: a spanning tree with
/// <c>Edges.Count == Nodes.Count - 1</c> in which every cell is reachable from every other cell.
/// </remarks>
public sealed class GrowingTreeLayoutGenerator : ILayoutGenerator
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
    public LayoutStyle Style => LayoutStyle.GrowingTree;
    #endregion Properties

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
        var active = new List<GridNode>();

        var start = graph.NodeAt(new GridPoint(random.Next(height), random.Next(width)));
        visited[start.Row, start.Column] = true;
        active.Add(start);

        while (active.Count > 0)
        {
            var index = random.Next(active.Count);
            var cell = active[index];
            var next = PickUnvisitedNeighbour(graph, cell, visited, random);

            // A cell without a cell left to reach leaves the set.
            if (next is null)
            {
                active.RemoveAt(index);
                continue;
            }

            visited[next.Row, next.Column] = true;
            graph.Connect(cell, next);
            active.Add(next);
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Chooses one of the neighbours of the given cell that has not been visited yet. Every
    /// candidate is equally likely to be chosen.
    /// </summary>
    /// <param name="graph">The graph the neighbour belongs to.</param>
    /// <param name="node">The node whose neighbour is chosen.</param>
    /// <param name="visited">The cells that have been reached already.</param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <returns>The chosen neighbour, or <see langword="null"/> when there is none left.</returns>
    private static GridNode? PickUnvisitedNeighbour(
        GridGraph graph,
        GridNode node,
        bool[,] visited,
        Random random)
    {
        var candidates = new GridPoint[Directions.Length];
        var count = 0;

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

            candidates[count++] = new GridPoint(row, column);
        }

        if (count == 0)
        {
            return null;
        }

        return graph.NodeAt(candidates[random.Next(count)]);
    }
    #endregion Private Methods
}
