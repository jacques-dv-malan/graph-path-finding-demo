using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Heuristic;

/// <summary>
/// Estimates the cost of a walk from the greatest of the two distances across the grid.
/// </summary>
/// <remarks>
/// A walk moves one row or one column at a time, so it takes at least as many steps as the
/// greater of the two distances, and every step costs at least one. The greater distance is
/// therefore a floor under the cost of any walk, and a looser one than the sum of the two.
/// </remarks>
public sealed class ChebyshevHeuristic : GridDistanceHeuristic
{
    #region Properties
    /// <inheritdoc />
    public override string Name => "Chebyshev";
    #endregion Properties

    #region Overridden Members
    /// <summary>
    /// Estimates the cost of the walk between two cells.
    /// </summary>
    /// <param name="from">The cell the walk starts at.</param>
    /// <param name="to">The cell the walk ends at.</param>
    /// <returns>The greater of the two distances across the grid.</returns>
    protected override double Estimate(GridNode from, GridNode to)
    {
        var rows = Math.Abs(from.Row - to.Row);
        var columns = Math.Abs(from.Column - to.Column);

        return GridMath.Estimate(Math.Max(rows, columns));
    }
    #endregion Overridden Members
}
