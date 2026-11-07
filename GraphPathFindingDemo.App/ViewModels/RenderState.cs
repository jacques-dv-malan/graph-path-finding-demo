using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.App.ViewModels;

public sealed record RenderState(
    GridScenario Scenario,
    IReadOnlyList<GridNode> Discovered,
    IReadOnlyList<GridNode> Visited,
    IReadOnlyList<GridNode> Path);
