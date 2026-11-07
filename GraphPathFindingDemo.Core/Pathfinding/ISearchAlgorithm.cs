using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// Searches a graph for a path between two nodes.
/// </summary>
public interface ISearchAlgorithm
{
    /// <summary>
    /// Searches the graph, reporting one <see cref="SearchStep"/> per event as it happens.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node the search starts from.</param>
    /// <param name="end">The node the search looks for.</param>
    /// <returns>The sequence of search steps. The sequence ends once the end node is reached.</returns>
    IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end);
}
