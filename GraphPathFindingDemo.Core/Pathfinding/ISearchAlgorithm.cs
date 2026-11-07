using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

public interface ISearchAlgorithm
{
    IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end);
}
