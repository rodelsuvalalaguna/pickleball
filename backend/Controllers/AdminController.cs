using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pickleball.Api.Data;
using Pickleball.Api.DTOs;
using Pickleball.Api.Models;
using Pickleball.Api.Services;

namespace Pickleball.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController(CourtService courts, BookingService bookings, AppDbContext db) : ControllerBase
{
    // Courts
    [HttpGet("courts")]
    public async Task<List<CourtDto>> Courts() => await courts.ListAsync(new CourtQuery(null, null, null, null), includeInactive: true);

    [HttpPost("courts")]
    public async Task<ActionResult<CourtDto>> CreateCourt(CourtUpsertDto dto) => StatusCode(201, await courts.CreateAsync(dto));

    [HttpPut("courts/{id:int}")]
    public async Task<CourtDto> UpdateCourt(int id, CourtUpsertDto dto) => await courts.UpdateAsync(id, dto);

    [HttpDelete("courts/{id:int}")]
    public async Task<IActionResult> DeactivateCourt(int id)
    {
        await courts.DeactivateAsync(id);
        return NoContent();
    }

    // Reservations
    [HttpGet("reservations")]
    public async Task<List<ReservationDto>> Reservations([FromQuery] int? courtId, [FromQuery] DateTime? date) =>
        await bookings.ListAllAsync(courtId, date);

    // Maintenance
    [HttpGet("blocks")]
    public async Task<List<BlockDto>> Blocks() => await bookings.ListBlocksAsync();

    [HttpPost("blocks")]
    public async Task<ActionResult<BlockDto>> CreateBlock(CreateBlockDto dto) => StatusCode(201, await bookings.CreateBlockAsync(dto));

    [HttpDelete("blocks/{id:int}")]
    public async Task<IActionResult> DeleteBlock(int id)
    {
        await bookings.DeleteBlockAsync(id);
        return NoContent();
    }

    // Users
    [HttpGet("users")]
    public async Task<List<UserDto>> Users() =>
        await db.Users.AsNoTracking().OrderBy(u => u.Name).Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role, u.CreatedAt)).ToListAsync();

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, ChangeRoleDto dto)
    {
        if (id == int.Parse(User.FindFirst("sub")!.Value))
            throw new AppException(400, "You cannot change your own role.");
        var u = await db.Users.FindAsync(id) ?? throw new AppException(404, "User not found.");
        u.Role = dto.Role;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
