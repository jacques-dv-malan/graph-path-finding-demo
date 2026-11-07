using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Generates a maze of rectangular rooms joined by doors.
/// </summary>
/// <remarks>
/// The grid is cut into bands of rows and bands of columns, and every room is the crossing of
/// one row band and one column band, so the rooms tile the grid and every cell belongs to a
/// room. The cells of a room are joined to each other, which turns the room into an open space
/// that holds loops of its own. The rooms are then joined by doors: every room is joined to the
/// room before it, and the rooms of the first column are joined to the room above them, which
/// keeps the rooms reachable from one another. A share of the remaining walls is opened as well,
/// which adds more loops. The result is a graph with loops in which every cell is reachable
/// from every other cell.
/// </remarks>
public sealed class RoomAndCorridorLayoutGenerator : ILayoutGenerator
{
    #region Constants
    /// <summary>
    /// The chance of a wall between two rooms being opened as a door on top of the doors that
    /// keep the rooms reachable.
    /// </summary>
    private const double ExtraDoorChance = 0.3;
    #endregion Constants

    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.RoomAndCorridor;
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

        var rows = SplitIntoBands(random, height);
        var columns = SplitIntoBands(random, width);

        foreach (var row in rows)
        {
            foreach (var column in columns)
            {
                CarveRoom(graph, row, column);
            }
        }

        for (var index = 0; index < rows.Length; index++)
        {
            for (var band = 0; band < columns.Length; band++)
            {
                // The room is joined to the room before it, so the rooms of a band are a chain.
                if (band > 0)
                {
                    OpenSideDoor(graph, random, rows[index], columns[band - 1], columns[band]);
                }

                // The first room of a band is joined to the room above it, so the bands are
                // chained to each other as well.
                if (band == 0 && index > 0)
                {
                    OpenTopDoor(graph, random, rows[index - 1], rows[index], columns[band]);
                }

                // The remaining walls are opened as doors from time to time.
                if (band > 0 && random.NextDouble() < ExtraDoorChance)
                {
                    OpenSideDoor(graph, random, rows[index], columns[band - 1], columns[band]);
                }

                if (band == 0 && index > 0 && random.NextDouble() < ExtraDoorChance)
                {
                    OpenTopDoor(graph, random, rows[index - 1], rows[index], columns[band]);
                }
            }
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Joins the cells of a room to each other, which leaves the room open.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="row">The band of rows the room lies in.</param>
    /// <param name="column">The band of columns the room lies in.</param>
    private static void CarveRoom(GridGraph graph, Band row, Band column)
    {
        for (var current = row.Start; current <= row.End; current++)
        {
            for (var offset = column.Start; offset < column.End; offset++)
            {
                graph.Connect(
                    graph.NodeAt(new GridPoint(current, offset)),
                    graph.NodeAt(new GridPoint(current, offset + 1)));
            }
        }

        for (var offset = column.Start; offset <= column.End; offset++)
        {
            for (var current = row.Start; current < row.End; current++)
            {
                graph.Connect(
                    graph.NodeAt(new GridPoint(current, offset)),
                    graph.NodeAt(new GridPoint(current + 1, offset)));
            }
        }
    }

    /// <summary>
    /// Opens a door in the wall between two rooms that stand next to each other.
    /// </summary>
    /// <param name="graph">The graph the passage belongs to.</param>
    /// <param name="random">The source of randomness for the place of the door.</param>
    /// <param name="row">The band of rows both rooms lie in.</param>
    /// <param name="left">The band of columns of the room on the left.</param>
    /// <param name="right">The band of columns of the room on the right.</param>
    private static void OpenSideDoor(
        GridGraph graph,
        Random random,
        Band row,
        Band left,
        Band right)
    {
        var doorRow = random.Next(row.Start, row.End + 1);

        var before = graph.NodeAt(new GridPoint(doorRow, left.End));
        var after = graph.NodeAt(new GridPoint(doorRow, right.Start));

        if (!graph.IsConnected(before, after))
        {
            graph.Connect(before, after);
        }
    }

    /// <summary>
    /// Opens a door in the wall between two rooms that stand above each other.
    /// </summary>
    /// <param name="graph">The graph the passage belongs to.</param>
    /// <param name="random">The source of randomness for the place of the door.</param>
    /// <param name="top">The band of rows of the room above.</param>
    /// <param name="bottom">The band of rows of the room below.</param>
    /// <param name="column">The band of columns both rooms lie in.</param>
    private static void OpenTopDoor(
        GridGraph graph,
        Random random,
        Band top,
        Band bottom,
        Band column)
    {
        var doorColumn = random.Next(column.Start, column.End + 1);

        var before = graph.NodeAt(new GridPoint(top.End, doorColumn));
        var after = graph.NodeAt(new GridPoint(bottom.Start, doorColumn));

        if (!graph.IsConnected(before, after))
        {
            graph.Connect(before, after);
        }
    }

    /// <summary>
    /// Cuts the given length into bands of rows or columns.
    /// </summary>
    /// <param name="random">The source of randomness for the sizes of the bands.</param>
    /// <param name="length">The number of rows or columns to cut.</param>
    /// <returns>The bands, in order.</returns>
    private static Band[] SplitIntoBands(Random random, int length)
    {
        var count = Math.Min(length, random.Next(2, Math.Max(2, length / 6) + 1));
        var sizes = new int[count];

        for (var index = 0; index < count; index++)
        {
            sizes[index] = 1;
        }

        var extra = length - count;

        while (extra > 0)
        {
            sizes[random.Next(count)]++;
            extra--;
        }

        var bands = new Band[count];
        var start = 0;

        for (var index = 0; index < count; index++)
        {
            bands[index] = new Band(start, start + sizes[index] - 1);
            start += sizes[index];
        }

        return bands;
    }
    #endregion Private Methods

    #region Internal Classes
    /// <summary>
    /// A band of rows or columns, from its first cell to its last.
    /// </summary>
    /// <param name="Start">The first row or column of the band.</param>
    /// <param name="End">The last row or column of the band.</param>
    private readonly record struct Band(int Start, int End);
    #endregion Internal Classes
}
