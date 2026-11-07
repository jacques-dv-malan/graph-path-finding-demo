using GraphPathFindingDemo.Core.Layout;

namespace GraphPathFindingDemo.App.ViewModels;

/// <summary>
/// A layout the user can pick, together with the name shown in the list.
/// </summary>
/// <param name="Style">The style the option selects.</param>
/// <param name="Name">The name shown for the option.</param>
public sealed record LayoutOption(LayoutStyle Style, string Name)
{
    #region Public Methods
    /// <summary>
    /// Creates the option describing the given layout style.
    /// </summary>
    /// <param name="style">The style to describe.</param>
    /// <returns>The option describing the style.</returns>
    public static LayoutOption For(LayoutStyle style)
    {
        var name = style switch
        {
            LayoutStyle.RecursiveDivision => "Recursive division",
            LayoutStyle.RecursiveBacktracker => "Recursive backtracker",
            LayoutStyle.RandomizedPrim => "Randomized Prim",
            LayoutStyle.Eller => "Eller",
            LayoutStyle.Wilson => "Wilson",
            LayoutStyle.GrowingTree => "Growing tree",
            LayoutStyle.HuntAndKill => "Hunt-and-kill",
            LayoutStyle.Braided => "Braided",
            _ => style.ToString()
        };

        return new LayoutOption(style, name);
    }
    #endregion Public Methods
}
