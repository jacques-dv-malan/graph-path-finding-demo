using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.App.Controls;

/// <summary>
/// The arguments of a click on a cell of the grid.
/// </summary>
public sealed class CellClickedEventArgs : EventArgs
{
    #region Properties
    /// <summary>
    /// Gets the cell that was clicked.
    /// </summary>
    public GridPoint Point { get; }

    /// <summary>
    /// Gets the endpoint the click moves.
    /// </summary>
    public EndpointTarget Target { get; }
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="CellClickedEventArgs"/> class.
    /// </summary>
    /// <param name="point">The cell that was clicked.</param>
    /// <param name="target">The endpoint the click moves.</param>
    public CellClickedEventArgs(GridPoint point, EndpointTarget target)
    {
        Point = point;
        Target = target;
    }
    #endregion Constructor
}
