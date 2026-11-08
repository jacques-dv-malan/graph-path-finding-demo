using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout.Generator;

/// <summary>
/// Generates a maze by walking at random from every cell until the walk reaches the part of the
/// maze that has been built already.
/// </summary>
/// <remarks>
/// A walk that meets itself leaves a loop behind, so the loop is erased and the walk carries on
/// from where it met itself. What is left when the walk reaches the maze is a simple path, which
/// is added to the maze. Every cell is added with exactly one passage, so the result is a
/// perfect maze: a spanning tree with <c>Edges.Count == Nodes.Count - 1</c> in which every cell
/// is reachable from every other cell. Because the walk takes every route equally often, the
/// maze is drawn evenly from all the mazes the grid allows.
/// </remarks>
public sealed class WilsonLayoutGenerator : ILayoutGenerator
{
    #region Constants
    /// <summary>
    /// The directions a walk may take.
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
    public LayoutStyle Style => LayoutStyle.Wilson;
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
        var inMaze = new bool[height, width];
        var positionOnPath = new int[graph.Nodes.Count];

        Array.Fill(positionOnPath, -1);

        // The cells the walks start from, in a random order.
        var order = new List<GridNode>(graph.Nodes);

        for (var index = order.Count - 1; index > 0; index--)
        {
            var swap = random.Next(index + 1);
            (order[index], order[swap]) = (order[swap], order[index]);
        }

        // The first cell of the maze is the one every walk eventually runs into.
        var root = order[0];
        inMaze[root.Row, root.Column] = true;

        for (var index = 1; index < order.Count; index++)
        {
            var start = order[index];

            // The cell may have joined the maze as part of an earlier walk.
            if (inMaze[start.Row, start.Column])
            {
                continue;
            }

            Walk(graph, random, inMaze, positionOnPath, start);
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Walks at random from the given cell until the walk reaches the maze, and adds the path it
    /// took to the maze.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="random">The source of randomness for the walk.</param>
    /// <param name="inMaze">The cells that belong to the maze already.</param>
    /// <param name="positionOnPath">The place every cell of the current path sits at, or
    /// <c>-1</c> when the cell is not on the path.</param>
    /// <param name="start">The cell the walk starts at.</param>
    private static void Walk(
        GridGraph graph,
        Random random,
        bool[,] inMaze,
        int[] positionOnPath,
        GridNode start)
    {
        var path = new List<GridNode> { start };
        var current = start;

        positionOnPath[start.Index] = 0;

        while (true)
        {
            var next = PickNeighbour(graph, current, random);

            if (inMaze[next.Row, next.Column])
            {
                AddPath(graph, inMaze, positionOnPath, path, next);
                return;
            }

            var position = positionOnPath[next.Index];

            if (position >= 0)
            {
                // The walk has met itself, so the loop is erased and the walk carries on.
                for (var index = path.Count - 1; index > position; index--)
                {
                    positionOnPath[path[index].Index] = -1;
                }

                path.RemoveRange(position + 1, path.Count - position - 1);
            }
            else
            {
                positionOnPath[next.Index] = path.Count;
                path.Add(next);
            }

            current = next;
        }
    }

    /// <summary>
    /// Adds the path a walk took to the maze, joining every cell of the path to the next one and
    /// the last cell to the maze.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="inMaze">The cells that belong to the maze already.</param>
    /// <param name="positionOnPath">The place every cell of the current path sits at.</param>
    /// <param name="path">The path the walk took.</param>
    /// <param name="last">The cell of the maze the walk reached.</param>
    private static void AddPath(
        GridGraph graph,
        bool[,] inMaze,
        int[] positionOnPath,
        List<GridNode> path,
        GridNode last)
    {
        for (var index = 0; index + 1 < path.Count; index++)
        {
            graph.Connect(path[index], path[index + 1]);
        }

        if (path.Count > 0)
        {
            graph.Connect(path[^1], last);
        }

        foreach (var node in path)
        {
            inMaze[node.Row, node.Column] = true;
            positionOnPath[node.Index] = -1;
        }
    }

    /// <summary>
    /// Chooses one of the neighbours of the given cell at random.
    /// </summary>
    /// <param name="graph">The graph the neighbour belongs to.</param>
    /// <param name="node">The node whose neighbour is chosen.</param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <returns>The chosen neighbour.</returns>
    private static GridNode PickNeighbour(GridGraph graph, GridNode node, Random random)
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

            candidates[count++] = new GridPoint(row, column);
        }

        return graph.NodeAt(candidates[random.Next(count)]);
    }
    #endregion Private Methods
}
