
namespace PickMeUp.Web.Models.Shared;

public class LocationLookupViewModel
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
    public CoordinatesViewModel Coordinates { get; set; } = default!;
}