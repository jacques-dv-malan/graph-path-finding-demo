using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// Searches a graph for a path between two nodes.
/// </summary>
public interface ISearchAlgorithm
{
    /// <summary>
    /// Gets the style of the search, which names it in the algorithm picker.
    /// </summary>
    SearchAlgorithmStyle Style { get; }

    /// <summary>
    /// Searches the graph, reporting one <see cref="SearchStep"/> per event as it happens.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node the search starts from.</param>
    /// <param name="end">The node the search looks for.</param>
    /// <returns>The sequence of search steps. The sequence ends once the end node is reached.</returns>
    /// <remarks>
    /// A move is the expansion of one node: the steps up to, but not including, the next
    /// <see cref="SearchStepKind.Visited"/> step, with the opening discovery included in the
    /// first move.
    /// </remarks>
    IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end);
}
