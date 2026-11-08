using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout.Generator;

/// <summary>
/// Generates a maze of caves by letting a cellular automaton settle the walls.
/// </summary>
/// <remarks>
/// Every cell starts out as a wall or as open ground, at random. A cell becomes a wall when it
/// has four or five wall cells around it, and open ground otherwise. The rule is applied over and
/// over until the walls stop moving, which leaves the walls in organic blobs and the open
/// ground in winding caves. The cells of a cave are joined to each other, and the caves are
/// joined to each other by opening the wall cells that stand between them. The cells that stay
/// walls are left without any passage, so they stand as rock. The result is a graph with loops
/// in which the caves are all reachable from one another.
/// </remarks>
public sealed class CellularAutomataLayoutGenerator : ILayoutGenerator
{
    #region Constants
    /// <summary>
    /// The share of cells that start out as walls.
    /// </summary>
    private const double WallChance = 0.45;

    /// <summary>
    /// The number of times the rule is applied before the walls are left as they are.
    /// </summary>
    private const int MaxIterations = 100;

    /// <summary>
    /// The cells around a cell that count towards the rule.
    /// </summary>
    private static readonly (int Row, int Column)[] Surroundings =
    [
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1), (0, 1),
        (1, -1), (1, 0), (1, 1)
    ];

    /// <summary>
    /// The cells around a cell that a passage can run to.
    /// </summary>
    private static readonly (int Row, int Column)[] Sides =
    [
        (-1, 0), (0, 1), (1, 0), (0, -1)
    ];
    #endregion Constants

    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.CellularAutomataCaves;
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
        var random = new Random(seed);
        var walls = new bool[height, width];

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                walls[row, column] = random.NextDouble() < WallChance;
            }
        }

        Settle(walls, width, height);

        // The corners are the ends of a search, so they are always open ground.
        walls[0, 0] = false;
        walls[0, width - 1] = false;
        walls[height - 1, 0] = false;
        walls[height - 1, width - 1] = false;

        JoinCaves(walls, width, height);

        var graph = new GridGraph(width, height);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                if (walls[row, column])
                {
                    continue;
                }

                var cell = graph.NodeAt(new GridPoint(row, column));

                if (column + 1 < width && !walls[row, column + 1])
                {
                    graph.Connect(cell, graph.NodeAt(new GridPoint(row, column + 1)));
                }

                if (row + 1 < height && !walls[row + 1, column])
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
    /// Applies the rule until the walls stop moving.
    /// </summary>
    /// <param name="walls">The cells that are walls, row by row.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    private static void Settle(bool[,] walls, int width, int height)
    {
        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var moved = false;

            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    var isWall = CountWallNeighbours(walls, width, height, row, column) is >= 4 and <= 5;

                    if (isWall != walls[row, column])
                    {
                        walls[row, column] = isWall;
                        moved = true;
                    }
                }
            }

            if (!moved)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Counts the cells around the given cell that are walls. The world outside the grid counts
    /// as a wall.
    /// </summary>
    /// <param name="walls">The cells that are walls, row by row.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    /// <param name="row">The row of the cell.</param>
    /// <param name="column">The column of the cell.</param>
    /// <returns>The number of wall cells around the cell.</returns>
    private static int CountWallNeighbours(bool[,] walls, int width, int height, int row, int column)
    {
        var count = 0;

        foreach (var (rowOffset, columnOffset) in Surroundings)
        {
            var neighbourRow = row + rowOffset;
            var neighbourColumn = column + columnOffset;

            if (neighbourRow < 0 || neighbourRow >= height
                || neighbourColumn < 0 || neighbourColumn >= width)
            {
                count++;
                continue;
            }

            if (walls[neighbourRow, neighbourColumn])
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Joins the caves to each other by opening the wall cells that stand between two of them.
    /// </summary>
    /// <param name="walls">The cells that are walls, row by row.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    private static void JoinCaves(bool[,] walls, int width, int height)
    {
        while (true)
        {
            var labels = new int[height, width];
            var regions = LabelCaves(walls, labels, width, height);

            if (regions <= 1)
            {
                return;
            }

            var opened = false;

            for (var row = 0; row < height && !opened; row++)
            {
                for (var column = 0; column < width && !opened; column++)
                {
                    if (!walls[row, column])
                    {
                        continue;
                    }

                    if (StandsBetweenCaves(labels, width, height, row, column))
                    {
                        walls[row, column] = false;
                        opened = true;
                    }
                }
            }

            if (!opened)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Numbers the caves, so that the cells of one cave share a number.
    /// </summary>
    /// <param name="walls">The cells that are walls, row by row.</param>
    /// <param name="labels">The cave every cell belongs to.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    /// <returns>The number of caves.</returns>
    private static int LabelCaves(bool[,] walls, int[,] labels, int width, int height)
    {
        var regions = 0;

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                if (walls[row, column] || labels[row, column] != 0)
                {
                    continue;
                }

                regions++;
                labels[row, column] = regions;
                FloodFill(walls, labels, width, height, row, column, regions);
            }
        }

        return regions;
    }

    /// <summary>
    /// Numbers the cells of one cave.
    /// </summary>
    /// <param name="walls">The cells that are walls, row by row.</param>
    /// <param name="labels">The cave every cell belongs to.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    /// <param name="row">The row of the cell the fill starts at.</param>
    /// <param name="column">The column of the cell the fill starts at.</param>
    /// <param name="label">The number of the cave.</param>
    private static void FloodFill(
        bool[,] walls,
        int[,] labels,
        int width,
        int height,
        int row,
        int column,
        int label)
    {
        var queue = new Queue<GridPoint>();
        queue.Enqueue(new GridPoint(row, column));

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();

            foreach (var (rowOffset, columnOffset) in Sides)
            {
                var neighbourRow = cell.Row + rowOffset;
                var neighbourColumn = cell.Column + columnOffset;

                if (neighbourRow < 0 || neighbourRow >= height
                    || neighbourColumn < 0 || neighbourColumn >= width)
                {
                    continue;
                }

                if (walls[neighbourRow, neighbourColumn] || labels[neighbourRow, neighbourColumn] != 0)
                {
                    continue;
                }

                labels[neighbourRow, neighbourColumn] = label;
                queue.Enqueue(new GridPoint(neighbourRow, neighbourColumn));
            }
        }
    }

    /// <summary>
    /// Determines whether the given wall cell stands between two caves.
    /// </summary>
    /// <param name="labels">The cave every cell belongs to.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    /// <param name="row">The row of the cell.</param>
    /// <param name="column">The column of the cell.</param>
    /// <returns><see langword="true"/> when the cell stands between two caves.</returns>
    private static bool StandsBetweenCaves(int[,] labels, int width, int height, int row, int column)
    {
        var first = 0;

        foreach (var (rowOffset, columnOffset) in Sides)
        {
            var neighbourRow = row + rowOffset;
            var neighbourColumn = column + columnOffset;

            if (neighbourRow < 0 || neighbourRow >= height
                || neighbourColumn < 0 || neighbourColumn >= width)
            {
                continue;
            }

            var label = labels[neighbourRow, neighbourColumn];

            if (label == 0)
            {
                continue;
            }

            if (first == 0)
            {
                first = label;
                continue;
            }

            if (label != first)
            {
                return true;
            }
        }

        return false;
    }
    #endregion Private Methods
}
