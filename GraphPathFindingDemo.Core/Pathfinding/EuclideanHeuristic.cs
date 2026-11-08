using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// Estimates the cost of a walk from the straight line between two cells.
/// </summary>
/// <remarks>
/// The straight line is the shortest way between two points in any shape, and a walk across the
/// grid takes at least as many steps as the distance across the grid, so the straight line is a
/// floor under the cost of any walk. It is the loosest of the distances here, because it forgets
/// that a walk has to step along the rows and the columns.
/// </remarks>
public sealed class EuclideanHeuristic : GridDistanceHeuristic
{
    #region Properties
    /// <inheritdoc />
    public override string Name => "Euclidean";
    #endregion Properties

    #region Overridden Members
    /// <summary>
    /// Estimates the cost of the walk between two cells.
    /// </summary>
    /// <param name="from">The cell the walk starts at.</param>
    /// <param name="to">The cell the walk ends at.</param>
    /// <returns>
    /// The straight line between the two cells, rounded down so that it never promises more than
    /// the truth.
    /// </returns>
    protected override int Estimate(GridNode from, GridNode to)
    {
        var rows = Math.Abs(from.Row - to.Row);
        var columns = Math.Abs(from.Column - to.Column);

        return (int)Math.Floor(Math.Sqrt((rows * rows) + (columns * columns)));
    }
    #endregion Overridden Members
}
