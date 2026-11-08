using GraphPathFindingDemo.Core.Graph;
using GraphPathFindingDemo.Core.Terrain;

namespace GraphPathFindingDemo.Core.Layout.Generator;

/// <summary>
/// Generates a perfect maze by taking the cheapest passages of the grid first, breaking ties at
/// random.
/// </summary>
/// <remarks>
/// Every passage between two orthogonal cells is a candidate, weighted by what the ground under
/// the two cells costs. Kruskal's algorithm sorts the candidates by cost and opens each one that
/// joins two cells that are not connected yet. The result is a perfect maze and a minimum spanning
/// tree of the weighted grid, so it minimises both the total climb over the whole maze and the
/// worst single climb. The ties between equally costly passages are broken by a random rank taken
/// from the same seed. Because a minimum spanning tree minimises the whole tree rather than one
/// route, the unique maze path between the endpoints is not guaranteed to be the cheapest route
/// through the full weighted grid.
/// </remarks>
public sealed class WeightedKruskalLayoutGenerator : IHeightAwareLayoutGenerator
{
    #region Properties
    /// <summary>
    /// Gets the style of the layout this generator produces.
    /// </summary>
    public LayoutStyle Style => LayoutStyle.WeightedKruskal;
    #endregion Properties

    #region Internal Classes
    /// <summary>
    /// A passage that may be opened, together with what it costs and the rank that settles ties.
    /// </summary>
    /// <param name="From">The first cell of the passage.</param>
    /// <param name="To">The second cell of the passage.</param>
    /// <param name="Cost">The cost of walking the passage.</param>
    /// <param name="Rank">A random value that orders passages of the same cost.</param>
    private readonly record struct Candidate(GridNode From, GridNode To, int Cost, int Rank);
    #endregion Internal Classes

    #region Public Methods
    /// <summary>
    /// Generates a maze for the requested grid from terrain derived from the same seed.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout. The same seed always
    /// produces the same maze.</param>
    /// <returns>The generated graph.</returns>
    /// <remarks>
    /// This overload keeps the generator usable through the base contract. The factory reaches
    /// the height-aware overload instead and applies the same terrain to the graph afterwards.
    /// </remarks>
    public GridGraph Generate(int width, int height, int seed)
    {
        return Generate(width, height, seed, HeightField.Generate(width, height, seed));
    }

    /// <summary>
    /// Generates a maze for the requested grid from the heights of its cells.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout. The same seed always
    /// produces the same maze.</param>
    /// <param name="heights">The height of every cell, row by row. There must be exactly one
    /// height for every node of the grid.</param>
    /// <returns>The generated graph.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="heights"/> is
    /// <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="heights"/> does not hold exactly one height for every node of the grid.
    /// </exception>
    public GridGraph Generate(int width, int height, int seed, IReadOnlyList<int> heights)
    {
        ArgumentNullException.ThrowIfNull(heights);

        if (heights.Count != width * height)
        {
            throw new ArgumentException(
                $"The grid holds {width * height} cells, but {heights.Count} heights were given.",
                nameof(heights));
        }

        var graph = new GridGraph(width, height);
        var random = new Random(seed);
        var candidates = new List<Candidate>(((width - 1) * height) + (width * (height - 1)));

        // The candidates are taken in a fixed order, right before down, so that the ranks drawn
        // from the seed are reproducible.
        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var node = graph.NodeAt(new GridPoint(row, column));

                if (column + 1 < width)
                {
                    AddCandidate(candidates, random, heights, node, graph.NodeAt(new GridPoint(row, column + 1)));
                }

                if (row + 1 < height)
                {
                    AddCandidate(candidates, random, heights, node, graph.NodeAt(new GridPoint(row + 1, column)));
                }
            }
        }

        // Cost alone would build the same low-cost skeleton for every seed, so the random rank
        // is part of the order as well.
        candidates.Sort((left, right) =>
        {
            var byCost = left.Cost.CompareTo(right.Cost);
            return byCost != 0 ? byCost : left.Rank.CompareTo(right.Rank);
        });

        var sets = new DisjointSets(width * height);

        foreach (var candidate in candidates)
        {
            if (graph.Edges.Count == graph.Nodes.Count - 1)
            {
                break;
            }

            var first = sets.Find(candidate.From.Index);
            var second = sets.Find(candidate.To.Index);

            if (first == second)
            {
                continue;
            }

            sets.Union(first, second);
            graph.Connect(candidate.From, candidate.To);
        }

        return graph;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Adds a candidate passage of the given cost to the list, drawing its tie-breaking rank from
    /// the seed.
    /// </summary>
    /// <param name="candidates">The candidate passages gathered so far.</param>
    /// <param name="random">The source of the tie-breaking ranks.</param>
    /// <param name="heights">The height of every cell, row by row.</param>
    /// <param name="from">The first cell of the passage.</param>
    /// <param name="to">The second cell of the passage.</param>
    private static void AddCandidate(
        List<Candidate> candidates,
        Random random,
        IReadOnlyList<int> heights,
        GridNode from,
        GridNode to)
    {
        var cost = 1 + Math.Abs(heights[from.Index] - heights[to.Index]);

        candidates.Add(new Candidate(from, to, cost, random.Next()));
    }
    #endregion Private Methods
}
