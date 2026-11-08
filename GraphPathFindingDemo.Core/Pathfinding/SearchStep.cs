using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// A single event reported by a search algorithm.
/// </summary>
/// <param name="Node">The node the event relates to.</param>
/// <param name="Kind">The kind of event being reported.</param>
/// <param name="Distance">The distance from the start node, when the algorithm tracks one. A
/// breadth-first search reports the length of the shortest way to the node; a depth-first search
/// reports the depth of the node in its own search tree. Each half of a bidirectional search
/// reports the distance from the end it grows from.</param>
/// <param name="Path">The full path from start to end. Only populated on
/// <see cref="SearchStepKind.PathFound"/>.</param>
public readonly record struct SearchStep(
    GridNode Node,
    SearchStepKind Kind,
    int? Distance,
    IReadOnlyList<GridNode>? Path = null);
