using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

public readonly record struct SearchStep(
    GridNode Node,
    SearchStepKind Kind,
    int? Distance,
    IReadOnlyList<GridNode>? Path = null);
