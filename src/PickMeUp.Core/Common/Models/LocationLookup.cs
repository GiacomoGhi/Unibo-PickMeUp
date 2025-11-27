namespace PickMeUp.Core.Common.Models;

public class LocationLookup
{
    /// <summary>
    /// Location identifier.
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// Full readable address of the location.
    /// </summary>
    public string ReadableAddress { get; set; } = default!;

    /// <summary>
    /// Coordinates of the location.
    /// </summary>
    public Coordinates Coordinates { get; set; } = default!;
}