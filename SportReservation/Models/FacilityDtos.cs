namespace SportReservation.Models;

public record FacilityDto(
    Guid Id,
    string Name,
    Guid TypeId,
    int Capacity,
    bool IsActive,
    DateTime CreatedAt
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
}