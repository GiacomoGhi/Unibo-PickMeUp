using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Web.Models.Shared;

public class CoordinatesViewModel
{
    /// <summary>
    /// Latitude component of the coordinates.
    /// </summary>
    [Required]
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude component of the coordinates.
    /// </summary>
    [Required]
    public double Longitude { get; set; }
}