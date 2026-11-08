using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Heuristic;

/// <summary>
/// Estimates the cost of a walk from the distance across the grid.
/// </summary>
/// <remarks>
/// A walk across the grid moves one row or one column at a time, so it takes at least as many
/// steps as the distance across the grid, and every step costs at least one. The distance is
/// therefore a floor under the cost of any walk.
/// </remarks>
public sealed class ManhattanHeuristic : GridDistanceHeuristic
{
    #region Properties
    /// <inheritdoc />
    public override string Name => "Manhattan";
    #endregion Properties

    #region Overridden Members
    /// <summary>
    /// Estimates the cost of the walk between two cells.
    /// </summary>
    /// <param name="from">The cell the walk starts at.</param>
    /// <param name="to">The cell the walk ends at.</param>
    /// <returns>The distance across the grid between the two cells.</returns>
    protected override int Estimate(GridNode from, GridNode to)
    {
        var rows = Math.Abs(from.Row - to.Row);
        var columns = Math.Abs(from.Column - to.Column);

        return rows + columns;
    }
    #endregion Overridden Members
}
