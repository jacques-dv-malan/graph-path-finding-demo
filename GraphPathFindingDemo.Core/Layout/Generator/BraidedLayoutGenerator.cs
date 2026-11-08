using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout.Generator;

/// <summary>
/// Generates a maze by taking a perfect maze and knocking the walls out of its dead ends.
/// </summary>
/// <remarks>
/// A dead end is a cell with a single passage. Opening a second passage at every dead end joins
/// the far side of the dead end to the maze around it, which turns the maze from a spanning
/// tree into a graph with loops. The result is no longer a perfect maze: it holds more passages
/// than cells minus one, and two cells may be joined by more than one route. The search still
/// finds the shortest route, but several routes of the same length may lead to it.
/// </remarks>
public sealed class BraidedLayoutGenerator : ILayoutGenerator
{
    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.Braided;
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
        var graph = new RecursiveBacktrackerLayoutGenerator().Generate(width, height, seed);
        var random = new Random(seed);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var cell = graph.NodeAt(new GridPoint(row, column));

                // Only a cell with a single passage is a dead end.
                if (graph.Neighbors(cell).Count != 1)
                {
                    continue;
                }

                var candidates = new List<GridNode>();

                foreach (var neighbour in Neighbours(graph, cell))
                {
                    if (!graph.IsConnected(cell, neighbour))
                    {
                        candidates.Add(neighbour);
                    }
                }

                if (candidates.Count == 0)
                {
                    continue;
                }

                graph.Connect(cell, candidates[random.Next(candidates.Count)]);
            }
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Returns the cells around the given cell that lie on the grid.
    /// </summary>
    /// <param name="graph">The graph the cell belongs to.</param>
    /// <param name="cell">The cell whose neighbours are returned.</param>
    /// <returns>The neighbours of the cell, whether a passage runs to them or not.</returns>
    private static IEnumerable<GridNode> Neighbours(GridGraph graph, GridNode cell)
    {
        var row = cell.Row;
        var column = cell.Column;

        if (row > 0)
        {
            yield return graph.NodeAt(new GridPoint(row - 1, column));
        }

        if (column + 1 < graph.Width)
        {
            yield return graph.NodeAt(new GridPoint(row, column + 1));
        }

        if (row + 1 < graph.Height)
        {
            yield return graph.NodeAt(new GridPoint(row + 1, column));
        }

        if (column > 0)
        {
            yield return graph.NodeAt(new GridPoint(row, column - 1));
        }
    }
    #endregion Private Methods
}
