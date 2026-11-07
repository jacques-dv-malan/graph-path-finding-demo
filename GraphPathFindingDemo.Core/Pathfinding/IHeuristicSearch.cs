namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// A search that can be tuned with a heuristic.
/// </summary>
public interface IHeuristicSearch
{
    /// <summary>
    /// Gets the heuristics the search can run with.
    /// </summary>
    IReadOnlyList<IHeuristic> Heuristics { get; }

    /// <summary>
    /// Gets or sets the heuristic the search runs with.
    /// </summary>
    IHeuristic Heuristic { get; set; }
}
