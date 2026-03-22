namespace SportReservation.Models;

public record ReservationDto(
    Guid Id,
    Guid UserId,
    Guid FacilityId,
    DateTime StartAt,
    DateTime EndAt,
    ReservationStatus Status,
    decimal BasePrice,
    int DiscountPercent,
    decimal FinalPrice,
    DateTime CreatedAt,
    DateTime? CancelledAt
);

public record AnonymousReservationDto(
    DateTime StartAt,
    DateTime EndAt
);

//možná do budoucna ?
public record UpdateReservationDto(
    DateTime? StartAt,
    DateTime? EndAt,
    Guid? FacilityId
);

public static class ReservationDtoExtensions
{
    public static ReservationDto ToDto(this Reservation r)
    {
        return new ReservationDto(
            Id: r.Id,
            UserId: r.UserId,
            FacilityId: r.FacilityId,
            StartAt: r.StartAt,
            EndAt: r.EndAt,
            Status: r.Status,
            BasePrice: r.BasePrice,
            DiscountPercent: r.DiscountPercent,
            FinalPrice: r.FinalPrice,
            CreatedAt: r.CreatedAt,
            CancelledAt: r.CancelledAt
        );
    }

    public static AnonymousReservationDto ToAnonymousDto(this Reservation reservation)
    {
        return new AnonymousReservationDto(
            reservation.StartAt,
            reservation.EndAt
        );
    }
}