using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Generates a maze by walking at random through the cells that have not been reached yet, and
/// hunting for a place to carry on when the walk runs into a dead end.
/// </summary>
/// <remarks>
/// The walk carves a passage to a cell that has not been reached yet and carries on from there,
/// until the cell it stands on has no cell left to reach. The hunt then looks over the grid for
/// the first cell that has not been reached yet but has a neighbour that has, joins that pair,
/// and the walk starts again from there. Every cell is reached by a single passage, so the result
/// is a perfect maze: a spanning tree with <c>Edges.Count == Nodes.Count - 1</c> in which every
/// cell is reachable from every other cell.
/// </remarks>
public sealed class HuntAndKillLayoutGenerator : ILayoutGenerator
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
    public LayoutStyle Style => LayoutStyle.HuntAndKill;
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

        var current = graph.NodeAt(new GridPoint(random.Next(height), random.Next(width)));
        visited[current.Row, current.Column] = true;

        while (true)
        {
            var walked = Walk(graph, random, visited, current);

            if (walked is not null)
            {
                current = walked;
                continue;
            }

            var hunted = Hunt(graph, random, visited);

            if (hunted is null)
            {
                break;
            }

            current = hunted;
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Walks from the given cell to a cell that has not been reached yet, joining the two.
    /// </summary>
    /// <param name="graph">The graph the passage belongs to.</param>
    /// <param name="random">The source of randomness for the walk.</param>
    /// <param name="visited">The cells that have been reached already.</param>
    /// <param name="current">The cell the walk starts at.</param>
    /// <returns>The cell the walk reached, or <see langword="null"/> when the walk is at a dead
    /// end.</returns>
    private static GridNode? Walk(
        GridGraph graph,
        Random random,
        bool[,] visited,
        GridNode current)
    {
        var directions = ShuffledDirections(random);

        foreach (var direction in directions)
        {
            var row = current.Row + direction.Row;
            var column = current.Column + direction.Column;

            if (row < 0 || row >= graph.Height || column < 0 || column >= graph.Width)
            {
                continue;
            }

            if (visited[row, column])
            {
                continue;
            }

            var next = graph.NodeAt(new GridPoint(row, column));
            visited[row, column] = true;
            graph.Connect(current, next);

            return next;
        }

        return null;
    }

    /// <summary>
    /// Looks over the grid for the first cell that has not been reached yet but has a neighbour
    /// that has, and joins that pair.
    /// </summary>
    /// <param name="graph">The graph the passage belongs to.</param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <param name="visited">The cells that have been reached already.</param>
    /// <returns>The cell that was reached, or <see langword="null"/> when no cell is left.</returns>
    private static GridNode? Hunt(
        GridGraph graph,
        Random random,
        bool[,] visited)
    {
        for (var row = 0; row < graph.Height; row++)
        {
            for (var column = 0; column < graph.Width; column++)
            {
                if (visited[row, column])
                {
                    continue;
                }

                var neighbours = new List<GridNode>();

                foreach (var direction in Directions)
                {
                    var neighbourRow = row + direction.Row;
                    var neighbourColumn = column + direction.Column;

                    if (neighbourRow < 0 || neighbourRow >= graph.Height
                        || neighbourColumn < 0 || neighbourColumn >= graph.Width)
                    {
                        continue;
                    }

                    if (visited[neighbourRow, neighbourColumn])
                    {
                        neighbours.Add(graph.NodeAt(new GridPoint(neighbourRow, neighbourColumn)));
                    }
                }

                if (neighbours.Count == 0)
                {
                    continue;
                }

                var cell = graph.NodeAt(new GridPoint(row, column));
                visited[row, column] = true;
                graph.Connect(cell, neighbours[random.Next(neighbours.Count)]);

                return cell;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the directions in an order of their own.
    /// </summary>
    /// <param name="random">The source of randomness for the order.</param>
    /// <returns>The directions, shuffled.</returns>
    private static GridPoint[] ShuffledDirections(Random random)
    {
        var directions = new GridPoint[Directions.Length];

        for (var index = 0; index < Directions.Length; index++)
        {
            directions[index] = Directions[index];
        }

        for (var index = directions.Length - 1; index > 0; index--)
        {
            var swap = random.Next(index + 1);
            (directions[index], directions[swap]) = (directions[swap], directions[index]);
        }

        return directions;
    }
    #endregion Private Methods
}
