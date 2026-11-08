using GraphPathFindingDemo.Core.Pathfinding;

namespace GraphPathFindingDemo.App.ViewModels;

/// <summary>
/// An algorithm the user can pick, together with the name shown in the list.
/// </summary>
/// <param name="Style">The style the option selects.</param>
/// <param name="Name">The name shown for the option.</param>
public sealed record SearchAlgorithmOption(SearchAlgorithmStyle Style, string Name)
{
    #region Public Methods
    /// <summary>
    /// Creates the option describing the given algorithm style.
    /// </summary>
    /// <param name="style">The style to describe.</param>
    /// <returns>The option describing the style.</returns>
    public static SearchAlgorithmOption For(SearchAlgorithmStyle style)
    {
        var name = style switch
        {
            SearchAlgorithmStyle.BreadthFirstSearch => "Breadth-first search",
            SearchAlgorithmStyle.DepthFirstSearch => "Depth-first search",
            SearchAlgorithmStyle.DijkstraSearch => "Dijkstra search",
            SearchAlgorithmStyle.AStarSearch => "A* search",
            SearchAlgorithmStyle.BidirectionalBreadthFirstSearch => "Bidirectional breadth-first search",
            SearchAlgorithmStyle.BidirectionalBreadthFirstDepthFirstSearch => "Bidirectional breadth-first / depth-first search",
            SearchAlgorithmStyle.BidirectionalDijkstraSearch => "Bidirectional Dijkstra search",
            SearchAlgorithmStyle.BidirectionalAStarSearch => "Bidirectional A* search",
            _ => style.ToString()
        };

        return new SearchAlgorithmOption(style, name);
    }
    #endregion Public Methods
}
