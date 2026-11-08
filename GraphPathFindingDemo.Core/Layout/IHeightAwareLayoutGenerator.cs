using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Builds the graph for a grid of a given size from terrain it is given up front, so that a
/// layout may take the cost of a passage into account while it is built.
/// </summary>
public interface IHeightAwareLayoutGenerator : ILayoutGenerator
{
    /// <summary>
    /// Generates a graph for the requested grid from the heights of its cells.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout.</param>
    /// <param name="heights">The height of every cell, row by row. There must be exactly one
    /// height for every node of the grid.</param>
    /// <returns>The generated graph.</returns>
    /// <remarks>
    /// The graph's own node heights and passage weights are applied by the factory afterwards,
    /// so a generator only needs the heights to choose which passages to open.
    /// </remarks>
    GridGraph Generate(int width, int height, int seed, IReadOnlyList<double> heights);
}
