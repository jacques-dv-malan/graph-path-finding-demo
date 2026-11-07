using GraphPathFindingDemo.Core.Endpoints;
using GraphPathFindingDemo.Core.Layout;

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
    public const int MaxSize = 60;
    #endregion Constants

    #region Instance Fields
    private readonly ILayoutGenerator _layouts;
    #endregion Instance Fields

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="GridFactory"/> class.
    /// </summary>
    /// <param name="layouts">The generator producing the graph of each scenario.</param>
    /// <exception cref="ArgumentNullException"><paramref name="layouts"/> is <see langword="null"/>.</exception>
    public GridFactory(ILayoutGenerator layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);
        _layouts = layouts;
    }
    #endregion Constructor

    #region Public Methods
    /// <summary>
    /// Creates a scenario for the requested grid.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout.</param>
    /// <param name="endpointMode">The mode deciding how the endpoints are chosen.</param>
    /// <returns>The created scenario.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> falls outside
    /// <see cref="MinSize"/> to <see cref="MaxSize"/>.
    /// </exception>
    public GridScenario Create(int width, int height, int seed, EndpointMode endpointMode = EndpointMode.FixedFixed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, MinSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, MinSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, MaxSize);

        var graph = _layouts.Generate(width, height, seed);
        var (start, end) = EndpointSelector.Select(graph, endpointMode, seed);

        return new GridScenario(graph, start, end, seed);
    }
    #endregion Public Methods
}
