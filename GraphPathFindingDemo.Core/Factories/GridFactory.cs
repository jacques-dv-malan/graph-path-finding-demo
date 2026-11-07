using GraphPathFindingDemo.Core.Endpoints;
using GraphPathFindingDemo.Core.Layout;

namespace GraphPathFindingDemo.Core.Factories;

public sealed class GridFactory : IGridFactory
{
    public const int MinSize = 5;
    public const int MaxSize = 60;

    private readonly ILayoutGenerator _layouts;

    public GridFactory(ILayoutGenerator layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);
        _layouts = layouts;
    }

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
}
