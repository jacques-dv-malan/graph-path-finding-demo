using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Factories;

/// <summary>
/// A generated grid together with the endpoints a search runs between.
/// </summary>
/// <param name="Graph">The generated graph.</param>
/// <param name="Start">The node the search starts from.</param>
/// <param name="End">The node the search looks for.</param>
/// <param name="Seed">The seed the graph was generated with.</param>
public sealed record GridScenario(GridGraph Graph, GridNode Start, GridNode End, int Seed);
