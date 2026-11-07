using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

public sealed class RecursiveDivisionLayoutGenerator : ILayoutGenerator
{
    public GridGraph Generate(int width, int height, int seed)
    {
        var graph = new GridGraph(width, height);
        var random = new Random(seed);

        var verticalWalls = new bool[width * height];
        var horizontalWalls = new bool[width * height];
        Array.Fill(verticalWalls, true);
        Array.Fill(horizontalWalls, true);

        Divide(0, height - 1, 0, width - 1);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var cell = graph.NodeAt(new GridPoint(row, column));

                if (column + 1 < width && !verticalWalls[row * width + column])
                {
                    graph.Connect(cell, graph.NodeAt(new GridPoint(row, column + 1)));
                }

                if (row + 1 < height && !horizontalWalls[row * width + column])
                {
                    graph.Connect(cell, graph.NodeAt(new GridPoint(row + 1, column)));
                }
            }
        }

        return graph;

        void Divide(int r0, int r1, int c0, int c1)
        {
            var canSplitVertically = c1 > c0;
            var canSplitHorizontally = r1 > r0;

            if (!canSplitVertically && !canSplitHorizontally)
            {
                return;
            }

            if (canSplitVertically && (!canSplitHorizontally || random.Next(2) == 0))
            {
                SplitVertically(r0, r1, c0, c1);
            }
            else
            {
                SplitHorizontally(r0, r1, c0, c1);
            }
        }

        void SplitVertically(int r0, int r1, int c0, int c1)
        {
            var wallColumn = random.Next(c0, c1);

            for (var row = r0; row <= r1; row++)
            {
                verticalWalls[row * width + wallColumn] = true;
            }

            var holeRow = random.Next(r0, r1 + 1);
            verticalWalls[holeRow * width + wallColumn] = false;

            Divide(r0, r1, c0, wallColumn);
            Divide(r0, r1, wallColumn + 1, c1);
        }

        void SplitHorizontally(int r0, int r1, int c0, int c1)
        {
            var wallRow = random.Next(r0, r1);

            for (var column = c0; column <= c1; column++)
            {
                horizontalWalls[wallRow * width + column] = true;
            }

            var holeColumn = random.Next(c0, c1 + 1);
            horizontalWalls[wallRow * width + holeColumn] = false;

            Divide(r0, wallRow, c0, c1);
            Divide(wallRow + 1, r1, c0, c1);
        }
    }
}
