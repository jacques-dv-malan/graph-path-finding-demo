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
    Dijkstra,

    /// <summary>
    /// The search takes the way that promises the cheapest way to the end, guided by a
    /// heuristic.
    /// </summary>
    AStar
}
