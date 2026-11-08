using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout.Generator;

/// <summary>
/// Generates a maze by taking a perfect maze and knocking out some of the walls it does not
/// need.
/// </summary>
/// <remarks>
/// The walls that are not knocked out are the ones a perfect maze needs to reach every cell from
/// every other cell, so the maze stays connected. Every other wall is a chance at an extra
/// passage, which turns the maze from a spanning tree into a graph with loops. The chance of a
/// wall being knocked out decides how open the maze is: a high chance leaves few walls, a low
/// chance leaves many.
/// </remarks>
public sealed class WallDensityLayoutGenerator : ILayoutGenerator
{
    #region Instance Fields
    private readonly double _openChance;
    #endregion Instance Fields

    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style { get; }

    /// <summary>
    /// Gets the chance of a wall the perfect maze does not need being knocked out.
    /// </summary>
    public double OpenChance => _openChance;
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="WallDensityLayoutGenerator"/> class.
    /// </summary>
    /// <param name="style">The style of the layout this generator produces.</param>
    /// <param name="openChance">The chance of a wall the perfect maze does not need being knocked
    /// out. A value close to <c>1</c> leaves few walls, a value close to <c>0</c> leaves
    /// many.</param>
    public WallDensityLayoutGenerator(LayoutStyle style, double openChance)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(openChance, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(openChance, 1);

        Style = style;
        _openChance = openChance;
    }
    #endregion Constructor

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

                if (column + 1 < width)
                {
                    OpenWall(graph, random, cell, graph.NodeAt(new GridPoint(row, column + 1)));
                }

                if (row + 1 < height)
                {
                    OpenWall(graph, random, cell, graph.NodeAt(new GridPoint(row + 1, column)));
                }
            }
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Knocks out the wall between the two given cells when the wall is not needed by the perfect
    /// maze and the dice fall that way.
    /// </summary>
    /// <param name="graph">The graph the passage belongs to.</param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <param name="cell">The cell the wall stands on.</param>
    /// <param name="neighbour">The cell on the other side of the wall.</param>
    private void OpenWall(GridGraph graph, Random random, GridNode cell, GridNode neighbour)
    {
        if (graph.IsConnected(cell, neighbour))
        {
            return;
        }

        if (random.NextDouble() < _openChance)
        {
            graph.Connect(cell, neighbour);
        }
    }
    #endregion Private Methods
}
