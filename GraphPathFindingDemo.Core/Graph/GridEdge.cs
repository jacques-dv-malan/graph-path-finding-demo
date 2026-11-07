namespace GraphPathFindingDemo.Core.Graph;

/// <summary>
/// A passage between two adjacent <see cref="GridNode"/> instances.
/// </summary>
/// <param name="From">The node the passage starts at.</param>
/// <param name="To">The node the passage ends at.</param>
public sealed record GridEdge(GridNode From, GridNode To)
{
    #region Properties
    /// <summary>
    /// Gets or sets the cost of traversing the passage. It starts at <c>1</c>, and the terrain
    /// sets it once the heights are known.
    /// </summary>
    public int Weight { get; internal set; } = 1;
    #endregion Properties
}
