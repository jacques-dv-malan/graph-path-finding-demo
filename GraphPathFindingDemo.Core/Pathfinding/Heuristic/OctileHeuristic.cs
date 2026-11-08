using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Heuristic;

/// <summary>
/// Estimates the cost of a walk from the distance across the grid and the way a walk steps.
/// </summary>
/// <remarks>
/// <para>
/// The estimate is the one of a grid that also allows stepping diagonally: a diagonal step covers
/// a row and a column at once and costs the length of the diagonal, so a walk over the two
/// distances costs the greater of them plus a share of the smaller one.
/// </para>
/// <para>
/// Our passages only ever run between cells that share a side, so the diagonal step does not
/// exist here and the estimate lands below the Manhattan distance. It is a floor under the cost
/// of any walk all the same, and it becomes the right bound the day diagonal passages are added.
/// </para>
/// </remarks>
public sealed class OctileHeuristic : GridDistanceHeuristic
{
    #region Constants
    /// <summary>
    /// What a diagonal step costs beyond a straight one.
    /// </summary>
    private static readonly double DiagonalSurcharge = Math.Sqrt(2) - 1;
    #endregion Constants

    #region Properties
    /// <inheritdoc />
    public override string Name => "Octile";
    #endregion Properties

    #region Overridden Members
    /// <summary>
    /// Estimates the cost of the walk between two cells.
    /// </summary>
    /// <param name="from">The cell the walk starts at.</param>
    /// <param name="to">The cell the walk ends at.</param>
    /// <returns>
    /// The greater of the two distances across the grid plus a share of the smaller one, rounded
    /// down so that it never promises more than the truth.
    /// </returns>
    protected override double Estimate(GridNode from, GridNode to)
    {
        var rows = Math.Abs(from.Row - to.Row);
        var columns = Math.Abs(from.Column - to.Column);

        var distance = Math.Max(rows, columns) + (DiagonalSurcharge * Math.Min(rows, columns));

        return GridMath.Estimate(distance);
    }
    #endregion Overridden Members
}
