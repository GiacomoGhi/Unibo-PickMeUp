using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PickMeUp.Core.Database.Models;

internal class Location
{
    /// <summary>
    /// Location identifier.
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// Full readable address.
    /// </summary>
    public string ReadableAddress { get; set; } = default!;

    /// <summary>
    /// Latitude coordinate of the departure location.
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// Longitude coordinate of the departure location.
    /// </summary>
    public double Longitude { get; set; }

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

internal class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        // Primary key
        builder.HasKey(e => e.LocationId);

        // Constraints
        builder.Property(e => e.ReadableAddress)
            .HasMaxLength(250);

        builder.Property(e => e.Street)
            .HasMaxLength(100);

        builder.Property(e => e.Number)
            .HasMaxLength(20);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.PostalCode)
            .HasMaxLength(20);

        builder.Property(e => e.Province)
            .HasMaxLength(100);

        builder.Property(e => e.Region)
            .HasMaxLength(100);

        builder.Property(e => e.Country)
            .HasMaxLength(100);

        builder.Property(e => e.Continent)
            .HasMaxLength(100);
    }
}