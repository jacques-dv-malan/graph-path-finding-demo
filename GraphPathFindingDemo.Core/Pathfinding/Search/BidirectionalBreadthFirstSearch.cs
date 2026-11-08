using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for a shortest path by growing breadth-first searches from both ends.
/// </summary>
/// <remarks>
/// The two searches advance level by level towards each other and stop as soon as the cheapest
/// meeting they can still reach cannot beat the best meeting found. Every passage is a plain step,
/// so the path found is the shortest one in cells.
/// </remarks>
public sealed class BidirectionalBreadthFirstSearch : ISearchAlgorithm
{
    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.BidirectionalBreadthFirstSearch;
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Searches the graph from both ends, reporting one <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node one search starts from.</param>
    /// <param name="end">The node the other search starts from.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the halves meet on the shortest
    /// path, or when both frontiers empty without meeting.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        return BidirectionalSearchDriver.Search(graph, start, end, useWeights: false, heuristic: null);
    }
    #endregion Public Methods
}
