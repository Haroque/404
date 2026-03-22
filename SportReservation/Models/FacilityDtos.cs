using SportReservation.Controllers;

namespace SportReservation.Models;

public record FacilityDto(
    Guid Id,
    string Name,
    Guid TypeId,
    int Capacity,
    bool IsActive,
    DateTime CreatedAt
);

public record FacilityComplexDto(
    Guid Id,
    string Name,
    FacilityTypeDto Type,
    int Capacity,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<DowntimeDto> DownTimes,
    IEnumerable<AnonymousReservationDto> ActiveReservations
);

public record FacilityCreateDto(
    string Name,
    Guid TypeId,
    int Capacity,
    bool IsActive
);

public static class FacilityDtoExtensions
{
    public static FacilityDto ToDto(this Facility facility)
    {
        return new FacilityDto(
            facility.Id,
            facility.Name,
            facility.TypeId,
            facility.Capacity,
            facility.IsActive,
            facility.CreatedAt
        );
    }

    public static FacilityComplexDto ToComplexDto(this Facility facility)
    {
        var now = DateTime.Now;

        return new FacilityComplexDto(
            facility.Id,
            facility.Name,
            facility.Type.ToDto(),
            facility.Capacity,
            facility.IsActive,
            facility.CreatedAt,
            facility.Downtimes.Select(x => x.ToDto()),
            facility.Reservations
                .Where(x => x.StartAt >= now && x.EndAt <= now)
                .Select(x => x.ToAnonymousDto())
        );
    }
}