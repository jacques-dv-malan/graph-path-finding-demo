using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// Estimates what it costs to walk from one node to another.
/// </summary>
/// <remarks>
/// An estimate has to stay under the truth: a search that trusts an estimate which is too high
/// can no longer promise the cheapest path.
/// </remarks>
public interface IHeuristic
{
    /// <summary>
    /// Gets the name of the estimate, shown in the heuristic picker.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Estimates what it costs to walk from one node to another.
    /// </summary>
    /// <param name="graph">The graph the two nodes belong to.</param>
    /// <param name="from">The node the walk starts at.</param>
    /// <param name="to">The node the walk ends at.</param>
    /// <returns>The estimated cost of the walk.</returns>
    int Estimate(GridGraph graph, GridNode from, GridNode to);
}
