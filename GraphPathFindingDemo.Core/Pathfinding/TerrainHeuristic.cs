using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

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
public sealed class TerrainHeuristic : IHeuristic
{
    #region Properties
    /// <inheritdoc />
    public string Name => "Terrain";
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Estimates what it costs to walk from one node to another.
    /// </summary>
    /// <param name="graph">The graph the two nodes belong to.</param>
    /// <param name="from">The node the walk starts at.</param>
    /// <param name="to">The node the walk ends at.</param>
    /// <returns>
    /// The distance across the grid plus the difference of the heights of the two cells.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public int Estimate(GridGraph graph, GridNode from, GridNode to)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var rows = Math.Abs(from.Row - to.Row);
        var columns = Math.Abs(from.Column - to.Column);
        var climb = Math.Abs(from.Height - to.Height);

        return rows + columns + climb;
    }
    #endregion Public Methods
}
