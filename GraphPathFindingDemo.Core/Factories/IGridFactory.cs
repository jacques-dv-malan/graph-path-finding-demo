using GraphPathFindingDemo.Core.Endpoints;

namespace GraphPathFindingDemo.Core.Factories;

public interface IGridFactory
{
    GridScenario Create(int width, int height, int seed, EndpointMode endpointMode = EndpointMode.FixedFixed);
}
