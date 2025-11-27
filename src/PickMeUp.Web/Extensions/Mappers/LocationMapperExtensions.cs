
using PickMeUp.Core.Common.Models;
using PickMeUp.Web.Models.Shared;

namespace PickMeUp.Web.Extensions.Mappers;

internal static class LocationMapperExtensions
{
    /// <summary>
    /// Maps a <see cref="Location"/> to a <see cref="LocationViewModel"/>.
    /// </summary>
    public static LocationViewModel ToViewModel(this Location source)
        => new()
        {
            LocationId = source.LocationId,
            ReadableAddress = source.ReadableAddress,
            Coordinates = new()
            {
                Latitude = source.Coordinates.Latitude,
                Longitude = source.Coordinates.Longitude
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

    /// <summary>
    /// Maps a <see cref="LocationViewModel"/> to a <see cref="Location"/>.
    /// </summary>
    public static Location ToServiceModel(this LocationViewModel source)
        => new()
        {
            LocationId = source.LocationId,
            ReadableAddress = source.ReadableAddress,
            Coordinates = new()
            {
                Latitude = source.Coordinates.Latitude,
                Longitude = source.Coordinates.Longitude
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

    /// <summary>
    /// Maps a <see cref="LocationLookup"/> to a <see cref="LocationLookupViewModel"/>.
    /// </summary>
    public static LocationLookupViewModel ToViewModel(this LocationLookup source)
        => new()
        {
            LocationId = source.LocationId,
            ReadableAddress = source.ReadableAddress,
            Coordinates = new()
            {
                Latitude = source.Coordinates.Latitude,
                Longitude = source.Coordinates.Longitude
            },
        };

    /// <summary>
    /// Maps a <see cref="LocationLookupViewModel"/> to a <see cref="LocationLookup"/>.
    /// </summary>
    public static LocationLookup ToServiceModel(this LocationLookupViewModel source)
        => new()
        {
            LocationId = source.LocationId,
            ReadableAddress = source.ReadableAddress,
            Coordinates = new()
            {
                Latitude = source.Coordinates.Latitude,
                Longitude = source.Coordinates.Longitude
            },
        };
}