namespace GraphPathFindingDemo.Core.Graph;

/// <summary>
/// A passage between two adjacent <see cref="GridNode"/> instances.
/// </summary>
/// <param name="From">The node the passage starts at.</param>
/// <param name="To">The node the passage ends at.</param>
/// <param name="Weight">The cost of traversing the passage. Defaults to <c>1</c>.</param>
public sealed record GridEdge(GridNode From, GridNode To, int Weight = 1);
