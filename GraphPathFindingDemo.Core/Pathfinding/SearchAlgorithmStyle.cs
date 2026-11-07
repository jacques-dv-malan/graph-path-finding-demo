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
    DepthFirstSearch
}
