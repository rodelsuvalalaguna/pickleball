using Microsoft.EntityFrameworkCore;
using Pickleball.Api.Models;

namespace Pickleball.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        if (!await db.Courts.AnyAsync())
        {
            db.Courts.AddRange(
                new Court { Name = "Court 1", Location = "Downtown Sports Hub", SurfaceType = "Hard", HourlyRate = 20, Description = "Indoor, climate controlled." },
                new Court { Name = "Court 2", Location = "Downtown Sports Hub", SurfaceType = "Hard", HourlyRate = 20, Description = "Indoor, climate controlled." },
                new Court { Name = "Riverside A", Location = "Riverside Park", SurfaceType = "Acrylic", HourlyRate = 15, Description = "Outdoor, lit for night play." },
                new Court { Name = "Riverside B", Location = "Riverside Park", SurfaceType = "Acrylic", HourlyRate = 15, Description = "Outdoor, lit for night play." },
                new Court { Name = "Clubhouse Court", Location = "Greenfield Club", SurfaceType = "Cushioned", HourlyRate = 25, Description = "Joint-friendly cushioned surface." });
        }

        // Admin credentials come from configuration (user secrets / env vars), never from source.
        var email = config["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = config["Seed:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)
            && !await db.Users.AnyAsync(u => u.Email == email))
        {
            db.Users.Add(new User
            {
                Name = "Administrator",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = Roles.Admin
            });
        }

        await db.SaveChangesAsync();
    }
}
