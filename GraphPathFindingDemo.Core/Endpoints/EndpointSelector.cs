using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Endpoints;

/// <summary>
/// Chooses the start and end nodes of a search according to an <see cref="EndpointMode"/>.
/// </summary>
public static class EndpointSelector
{
    #region Public Methods
    /// <summary>
    /// Selects the start and end nodes for the requested mode.
    /// </summary>
    /// <param name="graph">The graph the endpoints are selected in.</param>
    /// <param name="mode">The mode deciding how the endpoints are chosen.</param>
    /// <param name="seed">The seed used by the modes that choose nodes at random.</param>
    /// <returns>The chosen start and end nodes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">
    /// The requested mode is not implemented in this version.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">The mode is not a known value.</exception>
    public static (GridNode Start, GridNode End) Select(GridGraph graph, EndpointMode mode, int seed)
    {
        ArgumentNullException.ThrowIfNull(graph);

        return mode switch
        {
            EndpointMode.FixedFixed => SelectFixedFixed(graph),
            EndpointMode.FixedRandom => throw NotImplemented(mode),
            EndpointMode.RandomFixed => throw NotImplemented(mode),
            EndpointMode.RandomRandom => throw NotImplemented(mode),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown endpoint mode.")
        };
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Selects the top-left node as the start and the bottom-right node as the end.
    /// </summary>
    /// <param name="graph">The graph the endpoints are selected in.</param>
    /// <returns>The top-left and bottom-right nodes of the graph.</returns>
    /// <exception cref="ArgumentException">The start and end node are the same node.</exception>
    private static (GridNode Start, GridNode End) SelectFixedFixed(GridGraph graph)
    {
        var start = graph.NodeAt(new GridPoint(0, 0));
        var end = graph.NodeAt(new GridPoint(graph.Height - 1, graph.Width - 1));

        if (ReferenceEquals(start, end))
        {
            throw new ArgumentException("The start and end nodes must be different nodes.");
        }

        return (start, end);
    }

    /// <summary>
    /// Creates the exception raised for an endpoint mode that is not implemented yet.
    /// </summary>
    /// <param name="mode">The unsupported mode.</param>
    /// <returns>The exception to throw.</returns>
    private static NotSupportedException NotImplemented(EndpointMode mode)
    {
        return new NotSupportedException($"Endpoint mode '{mode}' is not implemented in v1.");
    }
    #endregion Private Methods
}
