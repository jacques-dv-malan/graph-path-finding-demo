namespace GraphPathFindingDemo.Core.Graph;

public readonly record struct GridPoint(int Row, int Column);

public sealed class GridNode
{
    public GridNode(int index, GridPoint position)
    {
        Index = index;
        Position = position;
    }

    public int Index { get; }

    public GridPoint Position { get; }

    public int Row => Position.Row;

    public int Column => Position.Column;
}
