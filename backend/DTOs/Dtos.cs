using System.ComponentModel.DataAnnotations;
using Pickleball.Api.Models;

namespace Pickleball.Api.DTOs;

// ---- Auth ----
public record RegisterDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 10)] string Password);

public record LoginDto(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100)] string Password);

public record RefreshDto([Required, StringLength(200)] string RefreshToken);

public record UserDto(int Id, string Name, string Email, string Role, DateTime CreatedAt);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record ChangeRoleDto([Required, RegularExpression("^(User|Admin)$")] string Role);

// ---- Courts ----
public record CourtDto(int Id, string Name, string Location, string SurfaceType, string? Description, decimal HourlyRate, bool IsActive);

public record CourtUpsertDto(
    [Required, StringLength(100)] string Name,
    [Required, StringLength(200)] string Location,
    [Required, StringLength(50)] string SurfaceType,
    [StringLength(1000)] string? Description,
    [Range(0.01, 10000)] decimal HourlyRate,
    bool IsActive = true);

public record CourtQuery(string? Location, string? SurfaceType, DateTime? Date, int? Hour);

public record SlotDto(DateTime Start, DateTime End, bool Available);

// ---- Reservations ----
public record CreateReservationDto(
    [Range(1, int.MaxValue)] int CourtId,
    DateTime StartTime,
    DateTime EndTime);

public record RescheduleDto(DateTime StartTime, DateTime EndTime);

public record ReservationDto(
    int Id, int CourtId, string CourtName, string CourtLocation,
    int UserId, string UserName,
    DateTime StartTime, DateTime EndTime, decimal TotalPrice,
    string Status, DateTime CreatedAt);

// ---- Maintenance ----
public record BlockDto(int Id, int CourtId, string CourtName, DateTime StartTime, DateTime EndTime, string Reason);

public record CreateBlockDto(
    [Range(1, int.MaxValue)] int CourtId,
    DateTime StartTime,
    DateTime EndTime,
    [Required, StringLength(200)] string Reason);

public static class Mapping
{
    public static UserDto ToDto(this User u) => new(u.Id, u.Name, u.Email, u.Role, u.CreatedAt);

    public static CourtDto ToDto(this Court c) =>
        new(c.Id, c.Name, c.Location, c.SurfaceType, c.Description, c.HourlyRate, c.IsActive);

    // Requires Court and User to be loaded.
    public static ReservationDto ToDto(this Reservation r) =>
        new(r.Id, r.CourtId, r.Court.Name, r.Court.Location, r.UserId, r.User.Name,
            r.StartTime, r.EndTime, r.TotalPrice, r.Status.ToString(), r.CreatedAt);
}
