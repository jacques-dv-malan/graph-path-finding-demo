using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Generates a maze of separate groves, each of them a maze of its own.
/// </summary>
/// <remarks>
/// The grid is cut into blocks that stand apart, with a wall between every two of them. Each
/// block is filled with a maze of its own, so the grid holds several trees that never touch.
/// The result is not a perfect maze: it falls short of the whole grid, so cells in one grove
/// cannot be reached from cells in another, and a search from one end of the grid to the other
/// ends without a path.
/// </remarks>
public sealed class ForestLayoutGenerator : ILayoutGenerator
{
    #region Constants
    /// <summary>
    /// The largest number of blocks a grid is cut into, along either axis.
    /// </summary>
    private const int MaxBlocks = 6;
    #endregion Constants

    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.Forest;
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

        var columnBlocks = SplitIntoBlocks(random, width, CountOfBlocks(random, width));
        var rowBlocks = SplitIntoBlocks(random, height, CountOfBlocks(random, height));

        foreach (var row in rowBlocks)
        {
            foreach (var column in columnBlocks)
            {
                FillBlock(graph, sets, random, row, column);
            }
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Returns the number of blocks the grid is cut into along one axis. The grid is cut into at
    /// least two blocks when there is room for them.
    /// </summary>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <param name="length">The number of rows or columns to cut.</param>
    /// <returns>The number of blocks.</returns>
    private static int CountOfBlocks(Random random, int length)
    {
        var most = Math.Min(MaxBlocks, (length + 1) / 2);

        if (most < 2)
        {
            return 1;
        }

        return random.Next(2, most + 1);
    }

    /// <summary>
    /// Cuts the given length into blocks that stand apart, with a gap of at least one cell
    /// between every two of them.
    /// </summary>
    /// <param name="random">The source of randomness for the sizes of the blocks.</param>
    /// <param name="length">The number of rows or columns to cut.</param>
    /// <param name="count">The number of blocks to cut the length into.</param>
    /// <returns>The blocks, in order.</returns>
    private static (int Start, int End)[] SplitIntoBlocks(Random random, int length, int count)
    {
        var sizes = new int[count];
        var gaps = new int[count - 1];

        for (var index = 0; index < count; index++)
        {
            sizes[index] = 1;
        }

        for (var index = 0; index < count - 1; index++)
        {
            gaps[index] = 1;
        }

        // The cells that are left after every block and every gap has one.
        var left = length - count - (count - 1);

        // Every block grows first, so the groves are of a fair size.
        var share = left / count;

        for (var index = 0; index < count; index++)
        {
            sizes[index] += share;
        }

        left -= share * count;

        // The rest is spread over the blocks and the gaps.
        while (left > 0)
        {
            if (count > 1 && random.Next(4) == 0)
            {
                gaps[random.Next(count - 1)]++;
            }
            else
            {
                sizes[random.Next(count)]++;
            }

            left--;
        }

        var blocks = new (int Start, int End)[count];
        var start = 0;

        for (var index = 0; index < count; index++)
        {
            blocks[index] = (start, start + sizes[index] - 1);
            start += sizes[index];

            if (index < count - 1)
            {
                start += gaps[index];
            }
        }

        return blocks;
    }

    /// <summary>
    /// Fills the given block with a maze of its own.
    /// </summary>
    /// <param name="graph">The graph the passages belong to.</param>
    /// <param name="sets">The groups the cells belong to, used to keep the maze free of loops.
    /// </param>
    /// <param name="random">The source of randomness for the choice.</param>
    /// <param name="row">The band of rows the block lies in.</param>
    /// <param name="column">The band of columns the block lies in.</param>
    private static void FillBlock(
        GridGraph graph,
        DisjointSets sets,
        Random random,
        (int Start, int End) row,
        (int Start, int End) column)
    {
        var passages = new List<GridNode[]>();

        for (var current = row.Start; current <= row.End; current++)
        {
            for (var offset = column.Start; offset < column.End; offset++)
            {
                passages.Add(
                [
                    graph.NodeAt(new GridPoint(current, offset)),
                    graph.NodeAt(new GridPoint(current, offset + 1))
                ]);
            }
        }

        for (var current = row.Start; current < row.End; current++)
        {
            for (var offset = column.Start; offset <= column.End; offset++)
            {
                passages.Add(
                [
                    graph.NodeAt(new GridPoint(current, offset)),
                    graph.NodeAt(new GridPoint(current + 1, offset))
                ]);
            }
        }

        // The passages are taken in a random order, and a passage is only opened when it joins
        // two cells that are not connected yet. The result is a maze with no loops.
        for (var index = passages.Count - 1; index > 0; index--)
        {
            var swap = random.Next(index + 1);
            (passages[index], passages[swap]) = (passages[swap], passages[index]);
        }

        foreach (var passage in passages)
        {
            var first = sets.Find(passage[0].Index);
            var second = sets.Find(passage[1].Index);

            if (first == second)
            {
                continue;
            }

            sets.Union(first, second);
            graph.Connect(passage[0], passage[1]);
        }
    }
    #endregion Private Methods

    #region Internal Classes
    /// <summary>
    /// The groups the cells belong to. Two cells are in the same group when they are connected
    /// to each other through the passages opened so far.
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
