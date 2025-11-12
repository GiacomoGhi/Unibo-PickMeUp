using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickMeUp.Enums.UserPickUpRequest;
using System;

namespace PickMeUp.Core.Database.Models;

internal class UserPickUpRequest
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int UserPickUpRequestId { get; set; }

    /// <summary>
    /// Identifier of the user associated with this pick up request.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Identifier of the travel that this request is for.
    /// </summary>
    public int UserTravelId { get; set; }

    /// <summary>
    /// Date and time for the pick up.
    /// </summary>
    public DateTime PickUpDateTime { get; set; }

    /// <summary>
    /// Latitude coordinate of the pick up point location.
    /// </summary>
    public double PickUpPointLatitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the pick up point location.
    /// </summary>
    public double PickUpPointLongitude { get; set; }

    /// <summary>
    /// Address of the pick up point location.
    /// </summary>
    public string PickUpPointAddress { get; set; } = default!;

    /// <summary>
    /// Distance in kilometers from the pick up point to the destination.
    /// </summary>
    public decimal PickUpPointToDestinationDistanceInKm { get; set; }

    /// <summary>
    /// Costs refund for the pick up that the user will pay to
    /// the driver.
    /// </summary>
    public decimal CostsRefund { get; set; }

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }

    /// <summary>
    /// Deletion date time of the pick up request.
    /// </summary>
    public DateTime? DeletionDateTime { get; set; }

    /// <summary>
    /// Navigation property to the associated user linked using <see cref="UserId"/>.
    /// </summary>
    public User User { get; set; } = default!;

    /// <summary>
    /// Navigation property to the associated travel linked using <see cref="UserTravelId"/>.
    /// </summary>
    public UserTravel UserTravel { get; set; } = default!;
}

internal class UserPickUpRequestConfiguration : IEntityTypeConfiguration<UserPickUpRequest>
{
    public void Configure(EntityTypeBuilder<UserPickUpRequest> builder)
    {
        // Primary key
        builder.HasKey(e => e.UserPickUpRequestId);

        // One-to-many relationship with User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

        // One-to-many relationship with UserTravel
        builder.HasOne(e => e.UserTravel)
            .WithMany()
            .HasForeignKey(e => e.UserTravelId);
    }
}