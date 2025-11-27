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
    /// Identifier of the departure location.
    /// </summary>
    public int DepartureLocationId { get; set; }

    /// <summary>
    /// Date and time of departure.
    /// </summary>
    public DateTime DepartureDateTime { get; set; }

    /// <summary>
    /// Identifier of the destination location.
    /// </summary>
    public int DestinationLocationId { get; set; }

    /// <summary>
    /// Date and time when this travel was deleted.
    /// </summary>
    public DateTime? DeletionDateTime { get; set; }

    /// <summary>
    /// Navigation property to the associated user linked using <see cref="UserId"/>.
    /// </summary>
    public User User { get; set; } = default!;

    /// <summary>
    /// Navigation property to the associated departure location linked using <see cref="DepartureLocationId"/>.
    /// </summary>
    public Location DepartureLocation { get; set; } = default!;

    /// <summary>
    /// Navigation property to the associated destination location linked using <see cref="DestinationLocationId"/>.
    /// </summary>
    public Location DestinationLocation { get; set; } = default!;
}

internal class UserTravelConfiguration : IEntityTypeConfiguration<UserTravel>
{
    public void Configure(EntityTypeBuilder<UserTravel> builder)
    {
        // Primary key
        builder.HasKey(e => e.UserTravelId);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.DepartureLocation)
            .WithMany()
            .HasForeignKey(e => e.DepartureLocationId);

        builder.HasOne(e => e.DestinationLocation)
            .WithMany()
            .HasForeignKey(e => e.DestinationLocationId);
    }
}
