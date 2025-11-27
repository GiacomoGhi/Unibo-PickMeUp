
using PickMeUp.Core.Common.Models;

namespace PickMeUp.Core.Common.Extensions;

internal static class LocationMapperExtensions
{
    /// <summary>
    /// Maps a <see cref="DatabaseModels.Location"/> to a <see cref="Location"/>.
    /// </summary>
    public static Location ToServiceModel(this DatabaseModels.Location source)
        => new()
        {
            LocationId = source.LocationId,
            ReadableAddress = source.ReadableAddress,
            Coordinates = new()
            {
                Latitude = source.Latitude,
                Longitude = source.Longitude
            },
            Street = source.Street,
            Number = source.Number,
            City = source.City,
            PostalCode = source.PostalCode,
            Province = source.Province,
            Region = source.Region,
            Country = source.Country,
            Continent = source.Continent,
        };
}