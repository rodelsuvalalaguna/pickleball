using Microsoft.EntityFrameworkCore;
using Pickleball.Api.Data;
using Pickleball.Api.DTOs;
using Pickleball.Api.Models;

namespace Pickleball.Api.Services;

public class AuthService(AppDbContext db, TokenService tokens)
{
    public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new AppException(409, "Email is already registered.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Roles.User
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return await IssueAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new AppException(401, "Invalid email or password.");
        return await IssueAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var hash = TokenService.Hash(refreshToken);
        var stored = await db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt < DateTime.UtcNow)
            throw new AppException(401, "Invalid refresh token.");

        stored.RevokedAt = DateTime.UtcNow; // rotate
        return await IssueAsync(stored.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hash = TokenService.Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null);
        if (stored is null) return;
        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private async Task<AuthResponse> IssueAsync(User user)
    {
        var (access, expires) = tokens.CreateAccessToken(user);
        var (refresh, hash, refreshExpires) = tokens.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = hash, ExpiresAt = refreshExpires });

        // Drop stale tokens for this user.
        var stale = await db.RefreshTokens
            .Where(t => t.UserId == user.Id && (t.ExpiresAt < DateTime.UtcNow || t.RevokedAt != null))
            .ToListAsync();
        db.RefreshTokens.RemoveRange(stale);

        await db.SaveChangesAsync();
        return new AuthResponse(access, refresh, expires, user.ToDto());
    }
}
