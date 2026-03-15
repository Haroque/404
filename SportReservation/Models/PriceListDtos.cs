namespace SportReservation.Models;

public record PriceListDto(
    Guid Id,
    Guid FacilityTypeId,
    DateTime ValidFrom,
    DateTime? ValidTo,
    decimal PricePerHour
);

public record PriceListCreateDto(
    Guid FacilityTypeId,
    DateTime ValidFrom,
    DateTime? ValidTo,
    decimal PricePerHour
);

public static class PriceListDtoExtensions
{
    public static PriceListDto ToDto(this PriceList type)
    {
        return new PriceListDto(
            type.Id,
            type.FacilityTypeId,
            type.ValidFrom,
            type.ValidTo,
            type.PricePerHour
        );
    }
}