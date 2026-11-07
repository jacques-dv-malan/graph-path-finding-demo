using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Factories;

public sealed record GridScenario(GridGraph Graph, GridNode Start, GridNode End, int Seed);
