namespace GraphPathFindingDemo.Core.Graph;

/// <summary>
/// A zero-based coordinate identifying a single cell on the grid.
/// </summary>
/// <param name="Row">The row index, where <c>0</c> is the top row.</param>
/// <param name="Column">The column index, where <c>0</c> is the leftmost column.</param>
public readonly record struct GridPoint(int Row, int Column);

/// <summary>
/// A single cell of a <see cref="GridGraph"/>. The node owns the coordinate it occupies.
/// </summary>
public sealed class GridNode
{
    #region Properties
    /// <summary>
    /// Gets the dense row-major index of this node, calculated as <c>row * width + column</c>.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the coordinate this node occupies.
    /// </summary>
    public GridPoint Position { get; }

    /// <summary>
    /// Gets the row of this node.
    /// </summary>
    public int Row => Position.Row;

    /// <summary>
    /// Gets the column index of this node.
    /// </summary>
    public int Column => Position.Column;

    /// <summary>
    /// Gets or sets the height of the ground this node stands on. The terrain sets it, and the
    /// search reads it to work out what a passage costs.
    /// </summary>
    public double Height { get; internal set; }
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="GridNode"/> class.
    /// </summary>
    /// <param name="index">The dense row-major index of the node.</param>
    /// <param name="position">The coordinate the node occupies.</param>
    public GridNode(int index, GridPoint position)
    {
        Index = index;
        Position = position;
    }
    #endregion Constructor
}
