namespace PickMeUp.Core.Common.Models;

public class Location
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

    /// <summary>
    /// Street name.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Street number.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Province name.
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// Region name.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Country name.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Continent name.
    /// </summary>
    public string? Continent { get; set; }
}