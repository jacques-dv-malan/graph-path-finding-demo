namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// The style of search an <see cref="ISearchAlgorithm"/> performs.
/// </summary>
public enum SearchAlgorithmStyle
{
    /// <summary>
    /// The search explores the graph level by level and finds a shortest path.
    /// </summary>
    BreadthFirstSearch,

    /// <summary>
    /// The search follows one branch as deep as it goes and finds any path.
    /// </summary>
    DepthFirstSearch,

    /// <summary>
    /// The search always takes the cheapest way out and finds the least costly path.
    /// </summary>
    DijkstraSearch,

    /// <summary>
    /// The search takes the way that promises the cheapest way to the end, guided by a
    /// heuristic.
    /// </summary>
    AStarSearch,

    /// <summary>
    /// Two searches grow level by level from the two ends and meet on a shortest path.
    /// </summary>
    BidirectionalBreadthFirstSearch,

    /// <summary>
    /// A breadth-first search from the start and a depth-first search from the end grow towards
    /// each other and are joined at the first cell they share.
    /// </summary>
    BidirectionalBreadthFirstDepthFirstSearch,

    /// <summary>
    /// Two searches grow towards each other, always taking the cheapest way out, and meet on the
    /// least costly path.
    /// </summary>
    BidirectionalDijkstraSearch,

    /// <summary>
    /// Two searches grow towards each other, guided by a heuristic, and meet on the least costly
    /// path.
    /// </summary>
    BidirectionalAStarSearch,
}
