using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pickleball.Api.Data;
using Pickleball.Api.DTOs;
using Pickleball.Api.Models;

namespace Pickleball.Api.Services;

public class BookingService(AppDbContext db, IOptions<BookingOptions> options)
{
    private readonly BookingOptions _o = options.Value;

    private DateTime Now()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(_o.TimeZone);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    // ---------- Availability ----------
    public async Task<List<SlotDto>> GetAvailabilityAsync(int courtId, DateTime date)
    {
        if (!await db.Courts.AnyAsync(c => c.Id == courtId && c.IsActive))
            throw new AppException(404, "Court not found.");

        var day = date.Date;
        var dayStart = day.AddHours(_o.OpenHour);
        var dayEnd = day.AddHours(_o.CloseHour);

        var busy = await db.Reservations.AsNoTracking()
            .Where(r => r.CourtId == courtId && r.Status == ReservationStatus.Confirmed && r.StartTime < dayEnd && r.EndTime > dayStart)
            .Select(r => new { r.StartTime, r.EndTime }).ToListAsync();
        var blocks = await db.MaintenanceBlocks.AsNoTracking()
            .Where(b => b.CourtId == courtId && b.StartTime < dayEnd && b.EndTime > dayStart)
            .Select(b => new { b.StartTime, b.EndTime }).ToListAsync();

        var now = Now();
        var slots = new List<SlotDto>();
        for (var h = _o.OpenHour; h < _o.CloseHour; h++)
        {
            var s = day.AddHours(h);
            var e = s.AddHours(1);
            var free = s > now
                       && !busy.Any(x => x.StartTime < e && x.EndTime > s)
                       && !blocks.Any(x => x.StartTime < e && x.EndTime > s);
            slots.Add(new SlotDto(s, e, free));
        }
        return slots;
    }

    // ---------- Create ----------
    public async Task<ReservationDto> CreateAsync(int userId, CreateReservationDto dto)
    {
        var (start, end) = ValidateRange(dto.StartTime, dto.EndTime);
        var court = await db.Courts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.CourtId && c.IsActive)
                    ?? throw new AppException(404, "Court not found.");

        var reservation = new Reservation
        {
            UserId = userId,
            CourtId = court.Id,
            StartTime = start,
            EndTime = end,
            TotalPrice = court.HourlyRate * (decimal)(end - start).TotalHours,
            Status = ReservationStatus.Confirmed
        };

        await InSerializableTransactionAsync(async () =>
        {
            await EnsureFreeAsync(court.Id, start, end, null);
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
        });

        return await LoadDtoAsync(reservation.Id);
    }

    // ---------- Reschedule ----------
    public async Task<ReservationDto> RescheduleAsync(int userId, int id, RescheduleDto dto)
    {
        var (start, end) = ValidateRange(dto.StartTime, dto.EndTime);

        await InSerializableTransactionAsync(async () =>
        {
            var r = await db.Reservations.Include(x => x.Court).FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)
                    ?? throw new AppException(404, "Reservation not found.");
            if (r.Status != ReservationStatus.Confirmed) throw new AppException(409, "Reservation is not active.");
            if (r.StartTime <= Now()) throw new AppException(409, "Past reservations cannot be changed.");

            await EnsureFreeAsync(r.CourtId, start, end, r.Id);
            r.StartTime = start;
            r.EndTime = end;
            r.TotalPrice = r.Court.HourlyRate * (decimal)(end - start).TotalHours;
            await db.SaveChangesAsync();
        });

        return await LoadDtoAsync(id);
    }

    // ---------- Cancel ----------
    public async Task CancelAsync(int userId, int id, bool isAdmin)
    {
        var r = await db.Reservations.FirstOrDefaultAsync(x => x.Id == id && (isAdmin || x.UserId == userId))
                ?? throw new AppException(404, "Reservation not found.");
        if (r.Status == ReservationStatus.Cancelled) return;
        if (!isAdmin && r.StartTime <= Now()) throw new AppException(409, "Past reservations cannot be cancelled.");
        r.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync();
    }

    // ---------- Queries ----------
    public async Task<List<ReservationDto>> ListForUserAsync(int userId) =>
        (await db.Reservations.AsNoTracking().Include(r => r.Court).Include(r => r.User)
            .Where(r => r.UserId == userId).OrderByDescending(r => r.StartTime).ToListAsync())
        .Select(r => r.ToDto()).ToList();

    public async Task<List<ReservationDto>> ListAllAsync(int? courtId, DateTime? date)
    {
        var q = db.Reservations.AsNoTracking().Include(r => r.Court).Include(r => r.User).AsQueryable();
        if (courtId is { } c) q = q.Where(r => r.CourtId == c);
        if (date is { } d) { var s = d.Date; var e = s.AddDays(1); q = q.Where(r => r.StartTime >= s && r.StartTime < e); }
        return (await q.OrderByDescending(r => r.StartTime).Take(500).ToListAsync()).Select(r => r.ToDto()).ToList();
    }

    // ---------- Maintenance blocks ----------
    public async Task<List<BlockDto>> ListBlocksAsync() =>
        await db.MaintenanceBlocks.AsNoTracking()
            .OrderByDescending(b => b.StartTime)
            .Select(b => new BlockDto(b.Id, b.CourtId, b.Court.Name, b.StartTime, b.EndTime, b.Reason))
            .Take(500).ToListAsync();

    public async Task<BlockDto> CreateBlockAsync(CreateBlockDto dto)
    {
        var start = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Unspecified);
        var end = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Unspecified);
        if (end <= start) throw new AppException(400, "End must be after start.");
        var court = await db.Courts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.CourtId)
                    ?? throw new AppException(404, "Court not found.");

        var block = new MaintenanceBlock { CourtId = court.Id, StartTime = start, EndTime = end, Reason = dto.Reason.Trim() };
        await InSerializableTransactionAsync(async () =>
        {
            if (await db.Reservations.AnyAsync(r => r.CourtId == court.Id && r.Status == ReservationStatus.Confirmed && r.StartTime < end && r.EndTime > start))
                throw new AppException(409, "Confirmed reservations exist in that window. Cancel them first.");
            db.MaintenanceBlocks.Add(block);
            await db.SaveChangesAsync();
        });
        return new BlockDto(block.Id, court.Id, court.Name, start, end, block.Reason);
    }

    public async Task DeleteBlockAsync(int id)
    {
        var b = await db.MaintenanceBlocks.FindAsync(id) ?? throw new AppException(404, "Block not found.");
        db.MaintenanceBlocks.Remove(b);
        await db.SaveChangesAsync();
    }

    // ---------- Helpers ----------
    private (DateTime Start, DateTime End) ValidateRange(DateTime start, DateTime end)
    {
        start = DateTime.SpecifyKind(start, DateTimeKind.Unspecified);
        end = DateTime.SpecifyKind(end, DateTimeKind.Unspecified);

        if (start.Minute != 0 || start.Second != 0 || end.Minute != 0 || end.Second != 0)
            throw new AppException(400, "Bookings must start and end on the hour.");
        var hours = (end - start).TotalHours;
        if (hours < 1 || hours > _o.MaxHours)
            throw new AppException(400, $"Duration must be between 1 and {_o.MaxHours} hours.");
        if (start.Date != end.Date && end.TimeOfDay != TimeSpan.Zero)
            throw new AppException(400, "Bookings cannot span multiple days.");
        if (start.Hour < _o.OpenHour || (end - start.Date).TotalHours > _o.CloseHour)
            throw new AppException(400, $"Courts are open {_o.OpenHour}:00 to {_o.CloseHour}:00.");
        if (start <= Now())
            throw new AppException(400, "Start time must be in the future.");
        return (start, end);
    }

    // Must run inside a Serializable transaction so the range read locks block concurrent inserts.
    private async Task EnsureFreeAsync(int courtId, DateTime start, DateTime end, int? excludeReservationId)
    {
        var clash = await db.Reservations.AnyAsync(r =>
            r.CourtId == courtId && r.Status == ReservationStatus.Confirmed
            && r.Id != excludeReservationId && r.StartTime < end && r.EndTime > start);
        if (clash) throw new AppException(409, "That time slot is no longer available.");

        var blocked = await db.MaintenanceBlocks.AnyAsync(b => b.CourtId == courtId && b.StartTime < end && b.EndTime > start);
        if (blocked) throw new AppException(409, "The court is closed for maintenance during that time.");
    }

    private async Task InSerializableTransactionAsync(Func<Task> work)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            await work();
            await tx.CommitAsync();
        }
        catch (Exception ex) when (IsDeadlock(ex))
        {
            // Two concurrent bookings on the same range: one is chosen as the victim.
            throw new AppException(409, "That time slot was just taken. Please pick another.");
        }
    }

    private static bool IsDeadlock(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
            if (ex is SqlException { Number: 1205 }) return true;
        return false;
    }

    private async Task<ReservationDto> LoadDtoAsync(int id)
    {
        db.ChangeTracker.Clear();
        var r = await db.Reservations.AsNoTracking().Include(x => x.Court).Include(x => x.User).FirstAsync(x => x.Id == id);
        return r.ToDto();
    }
}
