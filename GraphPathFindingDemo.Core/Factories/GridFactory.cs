using GraphPathFindingDemo.Core.Endpoints;
using GraphPathFindingDemo.Core.Layout;
using GraphPathFindingDemo.Core.Terrain;

namespace GraphPathFindingDemo.Core.Factories;

/// <summary>
/// Creates grid scenarios by combining a layout generator with the endpoint selector.
/// </summary>
public sealed class GridFactory : IGridFactory
{
    #region Constants
    /// <summary>
    /// The smallest number of rows or columns a grid may have.
    /// </summary>
    public const int MinSize = 5;

    /// <summary>
    /// The largest number of rows or columns a grid may have.
    /// </summary>
    public const int MaxSize = 100;
    #endregion Constants

    #region Instance Fields
    private readonly IReadOnlyList<LayoutStyle> _styles;
    private readonly IReadOnlyDictionary<LayoutStyle, ILayoutGenerator> _layouts;
    #endregion Instance Fields

    #region Properties
    /// <summary>
    /// Gets the layout styles the factory can build, in the order the generators were given.
    /// </summary>
    public IReadOnlyList<LayoutStyle> AvailableStyles => _styles;
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="GridFactory"/> class.
    /// </summary>
    /// <param name="layouts">The generators producing the graph of each scenario. Every
    /// generator must offer a different style.</param>
    /// <exception cref="ArgumentNullException"><paramref name="layouts"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// No generator was given, or two of them offer the same style.
    /// </exception>
    public GridFactory(params ILayoutGenerator[] layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);

        var generators = layouts.ToList();

        if (generators.Count == 0)
        {
            throw new ArgumentException("At least one layout generator is required.", nameof(layouts));
        }

        _styles = generators.Select(layout => layout.Style).ToArray();
        _layouts = generators.ToDictionary(layout => layout.Style);
    }
    #endregion Constructor

    #region Public Methods
    /// <summary>
    /// Creates a scenario for the requested grid.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout.</param>
    /// <param name="style">The style of the layout to generate.</param>
    /// <returns>The created scenario.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> falls outside
    /// <see cref="MinSize"/> to <see cref="MaxSize"/>, or no generator offers
    /// <paramref name="style"/>.
    /// </exception>
    public GridScenario Create(
        int width,
        int height,
        int seed,
        LayoutStyle style = LayoutStyle.RecursiveDivision)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, MinSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, MinSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, MaxSize);

        if (!_layouts.TryGetValue(style, out var layout))
        {
            throw new ArgumentOutOfRangeException(
                nameof(style),
                style,
                "No layout generator is registered for this style.");
        }

        // The ground the maze stands on decides what a passage costs, so the terrain is built
        // before the maze in case a layout takes those costs into account.
        var heights = HeightField.Generate(width, height, seed);
        var graph = layout is IHeightAwareLayoutGenerator heightAware
            ? heightAware.Generate(width, height, seed, heights)
            : layout.Generate(width, height, seed);

        graph.ApplyHeights(heights);

        var (start, end) = EndpointSelector.Select(graph);

        return new GridScenario(graph, start, end, seed);
    }
    #endregion Public Methods
}
