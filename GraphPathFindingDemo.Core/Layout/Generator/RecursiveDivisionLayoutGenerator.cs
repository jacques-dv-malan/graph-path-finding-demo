using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout.Generator;

/// <summary>
/// Generates a maze by recursively splitting the grid into rooms with walls and punching a
/// single opening into each wall.
/// </summary>
/// <remarks>
/// The result is a perfect maze: a spanning tree with <c>Edges.Count == Nodes.Count - 1</c> in
/// which every cell is reachable from every other cell.
/// </remarks>
public sealed class RecursiveDivisionLayoutGenerator : ILayoutGenerator
{
    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.RecursiveDivision;
    #endregion Properties

    #region Internal Classes
    /// <summary>
    /// Holds the wall bookkeeping of a single division run.
    /// </summary>
    /// <remarks>
    /// A vertical wall at <c>(row, column)</c> stands on the right edge of that cell, a
    /// horizontal wall at <c>(row, column)</c> on its bottom edge. Every edge starts out walled.
    /// </remarks>
    private sealed class Division
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Division"/> class with every edge walled.
        /// </summary>
        /// <param name="width">The number of columns to divide.</param>
        /// <param name="height">The number of rows to divide.</param>
        /// <param name="random">The source of randomness for the division.</param>
        public Division(int width, int height, Random random)
        {
            Random = random;
            VerticalWalls = new bool[height, width];
            HorizontalWalls = new bool[height, width];

            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    VerticalWalls[row, column] = true;
                    HorizontalWalls[row, column] = true;
                }
            }
        }

        /// <summary>
        /// Gets the source of randomness for the division.
        /// </summary>
        public Random Random { get; }

        /// <summary>
        /// Gets the vertical walls, indexed by row and column.
        /// </summary>
        public bool[,] VerticalWalls { get; }

        /// <summary>
        /// Gets the horizontal walls, indexed by row and column.
        /// </summary>
        public bool[,] HorizontalWalls { get; }
    }
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
        var division = new Division(width, height, new Random(seed));

        Divide(division, 0, height - 1, 0, width - 1);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var cell = graph.NodeAt(new GridPoint(row, column));

                if (column + 1 < width && !division.VerticalWalls[row, column])
                {
                    graph.Connect(cell, graph.NodeAt(new GridPoint(row, column + 1)));
                }

                if (row + 1 < height && !division.HorizontalWalls[row, column])
                {
                    graph.Connect(cell, graph.NodeAt(new GridPoint(row + 1, column)));
                }
            }
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Splits the given rectangle, choosing at random between the allowed directions.
    /// </summary>
    /// <param name="division">The wall bookkeeping of the run.</param>
    /// <param name="r0">The first row of the rectangle.</param>
    /// <param name="r1">The last row of the rectangle.</param>
    /// <param name="c0">The first column of the rectangle.</param>
    /// <param name="c1">The last column of the rectangle.</param>
    private void Divide(Division division, int r0, int r1, int c0, int c1)
    {
        var canSplitVertically = c1 > c0;
        var canSplitHorizontally = r1 > r0;

        if (!canSplitVertically && !canSplitHorizontally)
        {
            return;
        }

        if (canSplitVertically && (!canSplitHorizontally || division.Random.Next(2) == 0))
        {
            SplitVertically(division, r0, r1, c0, c1);
        }
        else
        {
            SplitHorizontally(division, r0, r1, c0, c1);
        }
    }

    /// <summary>
    /// Walls off a vertical line through the rectangle and opens a single hole in it.
    /// </summary>
    /// <param name="division">The wall bookkeeping of the run.</param>
    /// <param name="r0">The first row of the rectangle.</param>
    /// <param name="r1">The last row of the rectangle.</param>
    /// <param name="c0">The first column of the rectangle.</param>
    /// <param name="c1">The last column of the rectangle.</param>
    private void SplitVertically(Division division, int r0, int r1, int c0, int c1)
    {
        var wallColumn = division.Random.Next(c0, c1);

        for (var row = r0; row <= r1; row++)
        {
            division.VerticalWalls[row, wallColumn] = true;
        }

        var holeRow = division.Random.Next(r0, r1 + 1);
        division.VerticalWalls[holeRow, wallColumn] = false;

        Divide(division, r0, r1, c0, wallColumn);
        Divide(division, r0, r1, wallColumn + 1, c1);
    }

    /// <summary>
    /// Walls off a horizontal line through the rectangle and opens a single hole in it.
    /// </summary>
    /// <param name="division">The wall bookkeeping of the run.</param>
    /// <param name="r0">The first row of the rectangle.</param>
    /// <param name="r1">The last row of the rectangle.</param>
    /// <param name="c0">The first column of the rectangle.</param>
    /// <param name="c1">The last column of the rectangle.</param>
    private void SplitHorizontally(Division division, int r0, int r1, int c0, int c1)
    {
        var wallRow = division.Random.Next(r0, r1);

        for (var column = c0; column <= c1; column++)
        {
            division.HorizontalWalls[wallRow, column] = true;
        }

        var holeColumn = division.Random.Next(c0, c1 + 1);
        division.HorizontalWalls[wallRow, holeColumn] = false;

        Divide(division, r0, wallRow, c0, c1);
        Divide(division, wallRow + 1, r1, c0, c1);
    }
    #endregion Private Methods
}
