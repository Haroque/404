namespace SportReservation.Models;

public record FacilityTypeDto(
    Guid Id,
    string Name,
    string? Description,
    PriceListDto? CurrentPricing
);

public record FacilityTypeCreateDto(
    string Name,
    string? Description
);

public record FacilityTypePatchDto(
    Guid Id,
    string? Name,
    string? Description
);

public static class FacilityTypeDtoExtensions
{
    public static FacilityTypeDto ToDto(this FacilityType type)
    {
        var now = DateTime.Now;

        return new FacilityTypeDto(
            type.Id,
            type.Name,
            type.Description,
            type.PriceLists
                .FirstOrDefault(x => x.ValidFrom >= now && x.ValidTo <= now)
                ?.ToDto()
        );
    }
}