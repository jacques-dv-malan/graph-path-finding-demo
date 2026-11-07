using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Endpoints;

public static class EndpointSelector
{
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

    private static NotSupportedException NotImplemented(EndpointMode mode)
    {
        return new NotSupportedException($"Endpoint mode '{mode}' is not implemented in v1.");
    }
}
