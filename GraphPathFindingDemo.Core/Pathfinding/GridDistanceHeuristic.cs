using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// The base of the heuristics that measure how far two cells lie from each other across the grid.
/// </summary>
/// <remarks>
/// An estimate has to stay under the truth, or the search that trusts it can no longer promise
/// the cheapest path.
/// </remarks>
public abstract class GridDistanceHeuristic : IHeuristic
{
    #region Properties
    /// <inheritdoc />
    public abstract string Name { get; }
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Estimates what it costs to walk from one node to another.
    /// </summary>
    /// <param name="graph">The graph the two nodes belong to.</param>
    /// <param name="from">The node the walk starts at.</param>
    /// <param name="to">The node the walk ends at.</param>
    /// <returns>The estimated cost of the walk.</returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public int Estimate(GridGraph graph, GridNode from, GridNode to)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        return Estimate(from, to);
    }
    #endregion Public Methods

    #region Abstract Methods
    /// <summary>
    /// Estimates the cost of the walk between two cells.
    /// </summary>
    /// <param name="from">The cell the walk starts at.</param>
    /// <param name="to">The cell the walk ends at.</param>
    /// <returns>The estimated cost of the walk.</returns>
    protected abstract int Estimate(GridNode from, GridNode to);
    #endregion Abstract Methods
}
