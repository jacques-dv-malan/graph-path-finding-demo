namespace GraphPathFindingDemo.Core.Graph;

/// <summary>
/// Rounds passage costs and heuristic estimates to a fixed number of decimal places.
/// </summary>
/// <remarks>
/// A passage cost is rounded away from zero, so the value a search adds up is never below the
/// true cost. An estimate is rounded towards zero, so it never stands above the truth and a
/// search that trusts it can still promise the cheapest path.
/// </remarks>
internal static class GridMath
{
    #region Constants
    /// <summary>
    /// The number of decimal places a cost is kept at.
    /// </summary>
    private const int DecimalPlaces = 4;
    #endregion Constants

    #region Internal Methods
    /// <summary>
    /// Rounds a passage cost to four decimal places, never below the true value.
    /// </summary>
    /// <param name="value">The cost to round.</param>
    /// <returns>The rounded cost.</returns>
    internal static double Weight(double value)
    {
        return Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Rounds an estimate to four decimal places, never above the true value.
    /// </summary>
    /// <param name="value">The estimate to round.</param>
    /// <returns>The rounded estimate.</returns>
    internal static double Estimate(double value)
    {
        return Math.Round(value, DecimalPlaces, MidpointRounding.ToZero);
    }
    #endregion Internal Methods
}
