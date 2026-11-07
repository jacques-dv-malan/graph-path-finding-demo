using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Generates a maze row by row, joining the cells of a row into groups and letting every group
/// carry at least one passage into the next row.
/// </summary>
/// <remarks>
/// The groups track which cells are already connected to each other through the rows built so
/// far. Two cells are only joined by a passage when they are not connected yet, so the maze
/// never gains a loop. Every group carries at least one passage into the next row, so no group
/// is left behind and every cell stays reachable. The result is a perfect maze: a spanning tree
/// with <c>Edges.Count == Nodes.Count - 1</c> in which every cell is reachable from every other
/// cell.
/// </remarks>
public sealed class EllerLayoutGenerator : ILayoutGenerator
{
    #region Constants
    /// <summary>
    /// The chance of two neighbouring cells being joined by a passage.
    /// </summary>
    private const double JoinChance = 0.5;
    #endregion Constants

    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.Eller;
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
        var sets = new DisjointSets(width * height);

        for (var row = 0; row < height; row++)
        {
            var isLastRow = row == height - 1;

            JoinRow(graph, sets, random, row, width, isLastRow);

            if (!isLastRow)
            {
                CarryGroupsDown(graph, sets, random, row, width);
            }
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Joins the cells of the given row into groups, carving a passage wherever two neighbouring
    /// cells are not connected yet.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="sets">The groups the cells belong to.</param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <param name="row">The row being joined.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="isLastRow">Whether the row is the last one of the grid.</param>
    private static void JoinRow(
        GridGraph graph,
        DisjointSets sets,
        Random random,
        int row,
        int width,
        bool isLastRow)
    {
        for (var column = 0; column + 1 < width; column++)
        {
            var left = sets.Find(Index(row, column, width));
            var right = sets.Find(Index(row, column + 1, width));

            // Two cells that are connected already must not be joined again.
            if (left == right)
            {
                continue;
            }

            // The last row is joined in full, so that every group left reaches it.
            if (!isLastRow && random.NextDouble() >= JoinChance)
            {
                continue;
            }

            graph.Connect(
                graph.NodeAt(new GridPoint(row, column)),
                graph.NodeAt(new GridPoint(row, column + 1)));

            sets.Union(left, right);
        }
    }

    /// <summary>
    /// Carries the groups of the given row into the next one, giving every group at least one
    /// passage downwards so that it survives.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="sets">The groups the cells belong to.</param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <param name="row">The row the passages start at.</param>
    /// <param name="width">The number of columns of the grid.</param>
    private static void CarryGroupsDown(
        GridGraph graph,
        DisjointSets sets,
        Random random,
        int row,
        int width)
    {
        var groups = new Dictionary<int, List<int>>();

        for (var column = 0; column < width; column++)
        {
            var root = sets.Find(Index(row, column, width));

            if (!groups.TryGetValue(root, out var group))
            {
                group = [];
                groups.Add(root, group);
            }

            group.Add(column);
        }

        foreach (var group in groups.Values)
        {
            // A group of several cells passes on between one and all but one of its cells.
            var count = group.Count == 1 ? 1 : random.Next(1, group.Count);
            var remaining = new List<int>(group);

            for (var i = 0; i < count; i++)
            {
                var pick = random.Next(remaining.Count);
                var column = remaining[pick];
                remaining.RemoveAt(pick);

                graph.Connect(
                    graph.NodeAt(new GridPoint(row, column)),
                    graph.NodeAt(new GridPoint(row + 1, column)));

                sets.Union(sets.Find(Index(row, column, width)), sets.Find(Index(row + 1, column, width)));
            }
        }
    }

    /// <summary>
    /// Returns the index of the cell at the given coordinate.
    /// </summary>
    /// <param name="row">The row of the cell.</param>
    /// <param name="column">The column of the cell.</param>
    /// <param name="width">The number of columns of the grid.</param>
    /// <returns>The index of the cell.</returns>
    private static int Index(int row, int column, int width)
    {
        return (row * width) + column;
    }
    #endregion Private Methods

    #region Internal Classes
    /// <summary>
    /// The groups the cells belong to. Two cells are in the same group when they are connected to
    /// each other through the passages carved so far.
    /// </summary>
    private sealed class DisjointSets
    {
        #region Instance Fields
        private readonly int[] _parent;
        #endregion Instance Fields

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DisjointSets"/> class with every cell in
        /// a group of its own.
        /// </summary>
        /// <param name="count">The number of cells of the grid.</param>
        public DisjointSets(int count)
        {
            _parent = new int[count];

            for (var index = 0; index < count; index++)
            {
                _parent[index] = index;
            }
        }
        #endregion Constructor

        #region Public Methods
        /// <summary>
        /// Returns the group the given cell belongs to.
        /// </summary>
        /// <param name="index">The index of the cell.</param>
        /// <returns>The root of the group the cell belongs to.</returns>
        public int Find(int index)
        {
            while (_parent[index] != index)
            {
                _parent[index] = _parent[_parent[index]];
                index = _parent[index];
            }

            return index;
        }

        /// <summary>
        /// Puts the two given cells into the same group.
        /// </summary>
        /// <param name="a">The index of the first cell.</param>
        /// <param name="b">The index of the second cell.</param>
        public void Union(int a, int b)
        {
            var rootA = Find(a);
            var rootB = Find(b);

            if (rootA != rootB)
            {
                _parent[rootB] = rootA;
            }
        }
        #endregion Public Methods
    }
    #endregion Internal Classes
}
