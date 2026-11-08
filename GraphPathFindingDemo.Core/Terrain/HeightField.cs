namespace GraphPathFindingDemo.Core.Terrain;

/// <summary>
/// Builds the height of the ground under a grid, so that the passages across it do not all cost
/// the same.
/// </summary>
/// <remarks>
/// The heights are smooth: they follow a few hills that rise and fall over the grid, so a walk
/// over flat ground is cheap and a walk up a slope is dear.
/// </remarks>
public static class HeightField
{
    #region Constants
    /// <summary>
    /// The highest ground the terrain reaches.
    /// </summary>
    public const int MaxHeight = 6;

    /// <summary>
    /// The number of hills the coarse layer of the terrain is built from.
    /// </summary>
    private const int CoarseLatticeSize = 3;

    /// <summary>
    /// The number of hills the fine layer of the terrain is built from.
    /// </summary>
    private const int FineLatticeSize = 7;

    /// <summary>
    /// The share of the terrain the coarse layer accounts for.
    /// </summary>
    private const double CoarseShare = 0.7;
    #endregion Constants

    #region Public Methods
    /// <summary>
    /// Builds the height of every cell of a grid, row by row.
    /// </summary>
    /// <param name="width">The number of columns of the grid.</param>
    /// <param name="height">The number of rows of the grid.</param>
    /// <param name="seed">The seed the terrain is built from.</param>
    /// <returns>The height of every cell, in the order the nodes of the grid are created.</returns>
    public static double[] Generate(int width, int height, int seed)
    {
        var random = new Random(seed);
        var coarse = CreateLattice(random, CoarseLatticeSize);
        var fine = CreateLattice(random, FineLatticeSize);

        var values = new double[width * height];
        var lowest = double.MaxValue;
        var highest = double.MinValue;

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var x = Scale(column, width, CoarseLatticeSize - 1);
                var y = Scale(row, height, CoarseLatticeSize - 1);

                var value = (CoarseShare * Value(coarse, x, y))
                    + ((1 - CoarseShare) * Value(fine, x * 2, y * 2));

                var index = (row * width) + column;

                values[index] = value;
                lowest = Math.Min(lowest, value);
                highest = Math.Max(highest, value);
            }
        }

        // The hills are stretched over the whole range, so that the ground rises and falls as far
        // as it can.
        var span = Math.Max(double.Epsilon, highest - lowest);
        var heights = new double[values.Length];

        for (var index = 0; index < values.Length; index++)
        {
            heights[index] = ((values[index] - lowest) / span) * MaxHeight;
        }

        return heights;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Builds the random heights of the hills of one layer of the terrain.
    /// </summary>
    /// <param name="random">The source of the random heights.</param>
    /// <param name="size">The number of hills in a row.</param>
    /// <returns>The height of every hill.</returns>
    private static double[,] CreateLattice(Random random, int size)
    {
        var lattice = new double[size, size];

        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                lattice[row, column] = random.NextDouble();
            }
        }

        return lattice;
    }

    /// <summary>
    /// Maps a cell onto the hills of a layer of the terrain.
    /// </summary>
    /// <param name="index">The row or column of the cell.</param>
    /// <param name="count">The number of rows or columns of the grid.</param>
    /// <param name="steps">The number of hills the layer is built from.</param>
    /// <returns>The place of the cell among the hills.</returns>
    private static double Scale(int index, int count, int steps)
    {
        return (index / (double)Math.Max(1, count - 1)) * steps;
    }

    /// <summary>
    /// Returns the height of the ground at a place, taken from the hills around it.
    /// </summary>
    /// <param name="lattice">The heights of the hills.</param>
    /// <param name="x">The place along the hills.</param>
    /// <param name="y">The place across the hills.</param>
    /// <returns>The height of the ground at that place.</returns>
    private static double Value(double[,] lattice, double x, double y)
    {
        var size = lattice.GetLength(0);
        var left = (int)Math.Floor(x);
        var top = (int)Math.Floor(y);

        var along = Smooth(x - left);
        var across = Smooth(y - top);

        var upper = Interpolate(lattice[Clamp(top, size), Clamp(left, size)], lattice[Clamp(top, size), Clamp(left + 1, size)], along);
        var lower = Interpolate(lattice[Clamp(top + 1, size), Clamp(left, size)], lattice[Clamp(top + 1, size), Clamp(left + 1, size)], along);

        return Interpolate(upper, lower, across);
    }

    /// <summary>
    /// Eases a value in and out, so that the hills join without a crease.
    /// </summary>
    /// <param name="value">The value to ease.</param>
    /// <returns>The eased value.</returns>
    private static double Smooth(double value)
    {
        return value * value * (3 - (2 * value));
    }

    /// <summary>
    /// Moves from one height towards another.
    /// </summary>
    /// <param name="from">The height to start at.</param>
    /// <param name="to">The height to end at.</param>
    /// <param name="amount">How far to move, between <c>0</c> and <c>1</c>.</param>
    /// <returns>The height at that point.</returns>
    private static double Interpolate(double from, double to, double amount)
    {
        return from + ((to - from) * amount);
    }

    /// <summary>
    /// Keeps an index inside the hills.
    /// </summary>
    /// <param name="index">The index to keep.</param>
    /// <param name="size">The number of hills in a row.</param>
    /// <returns>The index inside the hills.</returns>
    private static int Clamp(int index, int size)
    {
        return Math.Clamp(index, 0, size - 1);
    }
    #endregion Private Methods
}
