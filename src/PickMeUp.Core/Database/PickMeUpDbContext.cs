using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Database.Models;

namespace PickMeUp.Core.Database;

internal class PickMeUpDbContext(DbContextOptions<PickMeUpDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPickUpRequest> UserPickUpRequests => Set<UserPickUpRequest>();
    public DbSet<UserTravel> UserTravels => Set<UserTravel>();
    public DbSet<Location> Locations => Set<Location>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PickMeUpDbContext).Assembly);

        // Apply entities configuration
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserPickUpRequestConfiguration());
        modelBuilder.ApplyConfiguration(new UserTravelConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
    }
}
