using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Heuristic;

/// <summary>
/// Estimates the cost of a walk from the distance across the grid and the climb between the two
/// cells.
/// </summary>
/// <remarks>
/// <para>
/// Every remaining step of a walk costs at least one, and a walk holds at least as many steps as
/// the distance across the grid. The climb it makes in total is at least the difference of the
/// heights of the two cells. The sum of the two is therefore a floor under the cost of any walk,
/// so the estimate never promises more than the truth.
/// </para>
/// <para>
/// The estimate is also steady: the estimate of a cell never stands above the cost of the step
/// out of it plus the estimate of the cell it leads to. A search that trusts it therefore never
/// has to look at a cell twice.
/// </para>
/// </remarks>
public sealed class TerrainHeuristic : GridDistanceHeuristic
{
    #region Properties
    /// <inheritdoc />
    public override string Name => "Terrain";
    #endregion Properties

    #region Overridden Members
    /// <summary>
    /// Estimates the cost of the walk between two cells.
    /// </summary>
    /// <param name="from">The cell the walk starts at.</param>
    /// <param name="to">The cell the walk ends at.</param>
    /// <returns>
    /// The distance across the grid plus the difference of the heights of the two cells.
    /// </returns>
    protected override double Estimate(GridNode from, GridNode to)
    {
        var rows = Math.Abs(from.Row - to.Row);
        var columns = Math.Abs(from.Column - to.Column);
        var climb = Math.Abs(from.Height - to.Height);

        return GridMath.Estimate(rows + columns + climb);
    }
    #endregion Overridden Members
}
