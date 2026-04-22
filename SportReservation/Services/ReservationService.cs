using Microsoft.EntityFrameworkCore;
using SportReservation.Models;
using SportReservation.Data;

namespace SportReservation.Services;

public class ReservationService
{
    private readonly AppDbContext _db;

    public ReservationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Reservation?> GetReservationAsync(Guid id)
    {
        return await _db.Reservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Reservation>> GetReservationsAsync(Guid? userId, Guid? facilityId, bool? active)
    {
        var q = _db.Reservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .AsQueryable();

        if (userId.HasValue)
        {
            q = q.Where(r => r.UserId == userId.Value);
        }

        if (facilityId.HasValue)
        {
            q = q.Where(r => r.FacilityId == facilityId.Value);
        }

        if (active.HasValue)
        {
            if (active.Value)
                q = q.Where(r => r.Status == ReservationStatus.Active);
            else
                q = q.Where(r => r.Status != ReservationStatus.Active);
        }

        return await q
            .OrderByDescending(r => r.StartAt)
            .ToListAsync();
    }

    public async Task<Reservation> CreateReservationAsync(Guid userId, Guid facilityId, DateTime startAt, DateTime endAt)
    {
        if (startAt < DateTime.Now)
            throw new BadHttpRequestException("past-slot");

        if (endAt <= startAt)
            throw new BadHttpRequestException("invalid-timespan");

        bool collision = await _db.Reservations.AnyAsync(r =>
            r.FacilityId == facilityId &&
            r.Status == ReservationStatus.Active &&
            r.StartAt < endAt &&
            r.EndAt > startAt);

        if (collision)
            throw new BadHttpRequestException("already-reserved");

        bool downtime = await _db.Downtimes.AnyAsync(d =>
            d.FacilityId == facilityId &&
            d.StartAt < endAt &&
            d.EndAt > startAt);

        if (downtime)
            throw new BadHttpRequestException("out-of-service");

        var facility = await _db.Facilities.FirstAsync(f => f.Id == facilityId);

        var priceList = await _db.PriceLists
            .Where(p => p.FacilityTypeId == facility.TypeId &&
                        p.ValidFrom <= startAt &&
                        (p.ValidTo == null || p.ValidTo >= endAt))
            .FirstOrDefaultAsync();

        if (priceList == null)
            throw new BadHttpRequestException("invalid-pricing");

        int reservationCount = await _db.Reservations.CountAsync(r =>
            r.UserId == userId &&
            r.Status == ReservationStatus.Active);

        int discount = 0;
        if (reservationCount >= 15)
            discount = 15;
        else if (reservationCount >= 10)
            discount = 10;
        else if (reservationCount >= 5)
            discount = 5;

        double hours = (endAt - startAt).TotalHours;
        decimal basePrice = Math.Round((decimal)hours * priceList.PricePerHour, 2);
        decimal finalPrice = Math.Round(basePrice * (1 - discount / 100m), 2);

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FacilityId = facilityId,
            StartAt = startAt,
            EndAt = endAt,
            Status = ReservationStatus.Active,
            BasePrice = basePrice,
            DiscountPercent = discount,
            FinalPrice = finalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();
        return reservation;
    }

    public async Task CancelReservationAsync(Guid reservationId, User user)
    {
        var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == reservationId);

        if (r == null)
            throw new BadHttpRequestException("not-found", StatusCodes.Status404NotFound);

        if (user.Role != UserRole.Admin && r.UserId != user.Id)
            throw new BadHttpRequestException("forbidden", StatusCodes.Status403Forbidden);

        if (user.Role != UserRole.Admin && r.StartAt.Date <= DateTime.Today)
            throw new BadHttpRequestException("same-day-cancel-not-allowed");

        r.CancelledAt = DateTime.UtcNow;
        r.Status = ReservationStatus.Cancelled;

        _db.Reservations.Update(r);
        await _db.SaveChangesAsync();
    }
}