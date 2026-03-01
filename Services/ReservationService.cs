using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportReservation.Data;
using SportReservation.Models;

namespace SportReservation.Services
{
    public class ReservationService
    {
        private readonly AppDbContext _db;

        public ReservationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Reservation> CreateReservationAsync(Guid userId, Guid facilityId, DateTime startAt, DateTime endAt)
        {
            // 1. Kontrola kolize – překrývající se aktivní rezervace
            bool collision = await _db.Reservations.AnyAsync(r =>
                r.FacilityId == facilityId &&
                r.Status == ReservationStatus.Active &&
                r.StartAt < endAt &&
                r.EndAt > startAt);

            if (collision)
                throw new Exception("Sportoviště je v tomto čase již rezervováno.");

            // 2. Kontrola odstávky
            bool downtime = await _db.Downtimes.AnyAsync(d =>
                d.FacilityId == facilityId &&
                d.StartAt < endAt &&
                d.EndAt > startAt);

            if (downtime)
                throw new Exception("Sportoviště je v tomto čase mimo provoz.");

            // 3. Aktuální ceník
            var priceList = await _db.PriceLists
                .Where(p => p.FacilityId == facilityId &&
                            p.ValidFrom <= startAt &&
                            (p.ValidTo == null || p.ValidTo >= endAt))
                .FirstOrDefaultAsync();

            if (priceList == null)
                throw new Exception("Pro sportoviště není nastaven ceník.");

            // 4. Sleva podle počtu předchozích rezervací (5/10/15 → 5/10/15%)
            int reservationCount = await _db.Reservations.CountAsync(r =>
                r.UserId == userId &&
                r.Status == ReservationStatus.Active);

            int discount = 0;
            if (reservationCount >= 15) discount = 15;
            else if (reservationCount >= 10) discount = 10;
            else if (reservationCount >= 5) discount = 5;

            // 5. Výpočet ceny
            double hours = (endAt - startAt).TotalHours;
            decimal basePrice = Math.Round((decimal)hours * priceList.PricePerHour, 2);
            decimal finalPrice = Math.Round(basePrice * (1 - discount / 100m), 2);

            // 6. Uložení
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

        public async Task CancelReservationAsync(Guid reservationId, Guid userId, bool isAdmin)
        {
            var reservation = await _db.Reservations.FindAsync(reservationId);

            if (reservation == null)
                throw new Exception("Rezervace nenalezena.");

            if (!isAdmin && reservation.UserId != userId)
                throw new Exception("Nemáte oprávnění zrušit tuto rezervaci.");

            reservation.Status = ReservationStatus.Cancelled;
            reservation.CancelledAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}