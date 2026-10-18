using Microsoft.AspNetCore.Mvc;
using Pickleball.Api.DTOs;
using Pickleball.Api.Services;

namespace Pickleball.Api.Controllers;

[ApiController]
[Route("api/courts")]
public class CourtsController(CourtService courts, BookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<List<CourtDto>> List([FromQuery] CourtQuery q) => await courts.ListAsync(q);

    [HttpGet("{id:int}")]
    public async Task<CourtDto> Get(int id) => await courts.GetAsync(id);

    [HttpGet("{id:int}/availability")]
    public async Task<List<SlotDto>> Availability(int id, [FromQuery] DateTime date) =>
        await bookings.GetAvailabilityAsync(id, date);
}
