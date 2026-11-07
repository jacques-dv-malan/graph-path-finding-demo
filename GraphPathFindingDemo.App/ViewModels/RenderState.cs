using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.App.ViewModels;

/// <summary>
/// The snapshot of a running search that the canvas draws.
/// </summary>
/// <remarks>
/// The record is replaced rather than mutated on every update, so that the canvas sees a new
/// value and repaints.
/// </remarks>
/// <param name="Scenario">The scenario being searched.</param>
/// <param name="Discovered">The nodes on the frontier, reached but not yet expanded.</param>
/// <param name="Visited">The nodes that have been expanded.</param>
/// <param name="Path">The path found so far. Empty until the search succeeds.</param>
public sealed record RenderState(
    GridScenario Scenario,
    IReadOnlyList<GridNode> Discovered,
    IReadOnlyList<GridNode> Visited,
    IReadOnlyList<GridNode> Path);
