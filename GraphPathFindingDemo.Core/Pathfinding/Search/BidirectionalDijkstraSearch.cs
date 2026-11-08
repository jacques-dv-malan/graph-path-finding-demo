using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for the cheapest path by growing Dijkstra searches from both ends.
/// </summary>
/// <remarks>
/// The two searches advance towards each other, each leaving the cheapest cell it knows. A passage
/// costs what the terrain says, so the path found is the least costly one even when a longer route
/// across flatter ground is cheaper.
/// </remarks>
public sealed class BidirectionalDijkstraSearch : ISearchAlgorithm
{
    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.BidirectionalDijkstraSearch;
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Searches the graph from both ends for the cheapest path, reporting one
    /// <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node one search starts from.</param>
    /// <param name="end">The node the other search starts from.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the halves meet on the cheapest
    /// path, or when both frontiers empty without meeting.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        return BidirectionalSearchDriver.Search(graph, start, end, useWeights: true, heuristic: null);
    }
    #endregion Public Methods
}
