using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Common.Helpers;
using PickMeUp.Core.Database;
using PickMeUp.Core.Database.Models;
using System.Threading.Tasks;

namespace PickMeUp.Core.Infrastructure;

internal static class DataSeeder
{
    /// <summary>
    /// Seed database with default data.
    /// If users already exist, no action is taken.
    /// </summary>
    public static async Task SeedDataAsync(PickMeUpDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var users = new[]
        {
            new User
            {
                Email = "john.doe@example.com",
                PasswordHash = CryptographyHelper.Hash("Password123!"),
                FirstName = "John",
                LastName = "Doe",
                NormalizedEmail = "JOHN.DOE@EXAMPLE.COM",
                IsEmailConfirmed = true,
            },
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
