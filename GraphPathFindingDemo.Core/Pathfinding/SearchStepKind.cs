namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// The kind of event reported by a <see cref="SearchStep"/>.
/// </summary>
public enum SearchStepKind
{
    /// <summary>
    /// The node was reached for the first time and added to the frontier.
    /// </summary>
    Discovered,

    /// <summary>
    /// The node was taken from the frontier and its neighbours were expanded.
    /// </summary>
    Visited,

    /// <summary>
    /// The end node was reached and the complete path is available.
    /// </summary>
    PathFound
}
