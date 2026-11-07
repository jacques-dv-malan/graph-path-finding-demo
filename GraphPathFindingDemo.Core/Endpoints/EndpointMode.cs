namespace GraphPathFindingDemo.Core.Endpoints;

/// <summary>
/// Determines how the start and end nodes of a search are chosen.
/// </summary>
public enum EndpointMode
{
    /// <summary>
    /// The start is the top-left node and the end is the bottom-right node.
    /// </summary>
    FixedFixed,

    /// <summary>
    /// The start is the top-left node and the end is chosen at random.
    /// </summary>
    FixedRandom,

    /// <summary>
    /// The start is chosen at random and the end is the bottom-right node.
    /// </summary>
    RandomFixed,

    /// <summary>
    /// Both the start and the end are chosen at random.
    /// </summary>
    RandomRandom
}
