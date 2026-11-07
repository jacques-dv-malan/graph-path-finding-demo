using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// Builds the graph for a grid of a given size.
/// </summary>
public interface ILayoutGenerator
{
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    LayoutStyle Style { get; }

    /// <summary>
    /// Generates a graph for the requested grid.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout.</param>
    /// <returns>The generated graph.</returns>
    GridGraph Generate(int width, int height, int seed);
}
