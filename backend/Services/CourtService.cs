using Microsoft.EntityFrameworkCore;
using Pickleball.Api.Data;
using Pickleball.Api.DTOs;
using Pickleball.Api.Models;

namespace Pickleball.Api.Services;

public class CourtService(AppDbContext db)
{
    public async Task<List<CourtDto>> ListAsync(CourtQuery q, bool includeInactive = false)
    {
        var query = db.Courts.AsNoTracking().AsQueryable();
        if (!includeInactive) query = query.Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(q.Location)) query = query.Where(c => c.Location.Contains(q.Location));
        if (!string.IsNullOrWhiteSpace(q.SurfaceType)) query = query.Where(c => c.SurfaceType == q.SurfaceType);

        var courts = await query.OrderBy(c => c.Location).ThenBy(c => c.Name).ToListAsync();

        // Date + hour: keep only courts that are free for that one-hour slot.
        if (q.Date is { } date && q.Hour is { } hour)
        {
            var start = DateTime.SpecifyKind(date.Date.AddHours(hour), DateTimeKind.Unspecified);
            var end = start.AddHours(1);
            var ids = courts.Select(c => c.Id).ToList();
            var booked = await db.Reservations
                .Where(r => ids.Contains(r.CourtId) && r.Status == ReservationStatus.Confirmed && r.StartTime < end && r.EndTime > start)
                .Select(r => r.CourtId).ToListAsync();
            var blocked = await db.MaintenanceBlocks
                .Where(b => ids.Contains(b.CourtId) && b.StartTime < end && b.EndTime > start)
                .Select(b => b.CourtId).ToListAsync();
            courts = courts.Where(c => !booked.Contains(c.Id) && !blocked.Contains(c.Id)).ToList();
        }

        return courts.Select(c => c.ToDto()).ToList();
    }

    public async Task<CourtDto> GetAsync(int id)
    {
        var c = await db.Courts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new AppException(404, "Court not found.");
        return c.ToDto();
    }

    public async Task<CourtDto> CreateAsync(CourtUpsertDto dto)
    {
        var c = new Court();
        Apply(c, dto);
        db.Courts.Add(c);
        await db.SaveChangesAsync();
        return c.ToDto();
    }

    public async Task<CourtDto> UpdateAsync(int id, CourtUpsertDto dto)
    {
        var c = await db.Courts.FindAsync(id) ?? throw new AppException(404, "Court not found.");
        Apply(c, dto);
        await db.SaveChangesAsync();
        return c.ToDto();
    }

    // Soft delete keeps reservation history intact.
    public async Task DeactivateAsync(int id)
    {
        var c = await db.Courts.FindAsync(id) ?? throw new AppException(404, "Court not found.");
        c.IsActive = false;
        await db.SaveChangesAsync();
    }

    private static void Apply(Court c, CourtUpsertDto d)
    {
        c.Name = d.Name.Trim();
        c.Location = d.Location.Trim();
        c.SurfaceType = d.SurfaceType.Trim();
        c.Description = d.Description?.Trim();
        c.HourlyRate = d.HourlyRate;
        c.IsActive = d.IsActive;
    }
}
