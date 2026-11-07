namespace GraphPathFindingDemo.Core.Graph;

public sealed record GridEdge(GridNode From, GridNode To, int Weight = 1);
