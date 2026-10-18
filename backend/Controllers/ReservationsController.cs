using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pickleball.Api.DTOs;
using Pickleball.Api.Models;
using Pickleball.Api.Services;

namespace Pickleball.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public class ReservationsController(BookingService bookings) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("mine")]
    public async Task<List<ReservationDto>> Mine() => await bookings.ListForUserAsync(UserId);

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto) =>
        StatusCode(201, await bookings.CreateAsync(UserId, dto));

    [HttpPut("{id:int}/reschedule")]
    public async Task<ReservationDto> Reschedule(int id, RescheduleDto dto) =>
        await bookings.RescheduleAsync(UserId, id, dto);

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await bookings.CancelAsync(UserId, id, User.IsInRole(Roles.Admin));
        return NoContent();
    }
}
