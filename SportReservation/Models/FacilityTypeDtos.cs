namespace SportReservation.Models;

public record FacilityTypeDto(
    Guid Id,
    string Name,
    string? Description
);

public record FacilityTypeCreateDto(
    string Name,
    string? Description
);

public static class FacilityTypeDtoExtensions
{
    public static FacilityTypeDto ToDto(this FacilityType type)
    {
        return new FacilityTypeDto(
            type.Id,
            type.Name,
            type.Description
        );
    }
}