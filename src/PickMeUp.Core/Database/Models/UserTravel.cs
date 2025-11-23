using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace PickMeUp.Core.Database.Models;

internal class UserTravel
{
    /// <summary>
    /// User travel identifier.
    /// </summary>
    public int UserTravelId { get; set; }

    /// <summary>
    /// Identifier of the user associated with this travel.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Total passenger seats count.
    /// </summary>
    public int TotalPassengersSeatsCount { get; set; }

    /// <summary>
    /// Count of occupied passenger seats.
    /// </summary>
    public int OccupiedPassengerSeatsCount { get; set; }

    /// <summary>
    /// Latitude coordinate of the departure location.
    /// </summary>
    public double DepartureLatitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the departure location.
    /// </summary>
    public double DepartureLongitude { get; set; }

    /// <summary>
    /// Address of the departure location.
    /// </summary>
    public string DepartureAddress { get; set; } = default!;

    /// <summary>
    /// Date and time of departure.
    /// </summary>
    public DateTime DepartureDateTime { get; set; }

    /// <summary>
    /// Latitude coordinate of the destination location.
    /// </summary>
    public double DestinationLatitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the destination location.
    /// </summary>
    public double DestinationLongitude { get; set; }

    /// <summary>
    /// Address of the destination location.
    /// </summary>
    public string DestinationAddress { get; set; } = default!;

    /// <summary>
    /// Date and time when this travel was deleted.
    /// </summary>
    public DateTime? DeletionDateTime { get; set; }

    /// <summary>
    /// Navigation property to the associated user linked using <see cref="UserId"/>.
    /// </summary>
    public User User { get; set; } = default!;
}

internal class UserTravelConfiguration : IEntityTypeConfiguration<UserTravel>
{
    public void Configure(EntityTypeBuilder<UserTravel> builder)
    {
        // Primary key
        builder.HasKey(e => e.UserTravelId);

        // Constraints
        builder.Property(e => e.DepartureAddress)
            .HasMaxLength(250);

        builder.Property(e => e.DestinationAddress)
            .HasMaxLength(250);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}
