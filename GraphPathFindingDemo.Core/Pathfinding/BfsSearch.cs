using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

public sealed class BfsSearch : ISearchAlgorithm
{
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var cameFrom = new Dictionary<GridNode, GridNode?>();
        var distances = new Dictionary<GridNode, int>();
        var queue = new Queue<GridNode>();

        cameFrom[start] = null;
        distances[start] = 0;
        queue.Enqueue(start);
        yield return new SearchStep(start, SearchStepKind.Discovered, 0);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            yield return new SearchStep(node, SearchStepKind.Visited, distances[node]);

            if (ReferenceEquals(node, end))
            {
                yield return new SearchStep(node, SearchStepKind.PathFound, distances[node], BuildPath(end, cameFrom));
                yield break;
            }

            foreach (var neighbour in graph.Neighbors(node))
            {
                if (cameFrom.ContainsKey(neighbour))
                {
                    continue;
                }

                cameFrom[neighbour] = node;
                distances[neighbour] = distances[node] + 1;
                queue.Enqueue(neighbour);
                yield return new SearchStep(neighbour, SearchStepKind.Discovered, distances[neighbour]);
            }
        }
    }

    private static IReadOnlyList<GridNode> BuildPath(
        GridNode end,
        Dictionary<GridNode, GridNode?> cameFrom)
    {
        var reversed = new List<GridNode>();
        GridNode? current = end;

        while (current is not null)
        {
            reversed.Add(current);
            current = cameFrom[current];
        }

        reversed.Reverse();
        return reversed;
    }
}
