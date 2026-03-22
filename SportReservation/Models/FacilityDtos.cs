namespace SportReservation.Models;

public record FacilityTypeDto(
    Guid Id,
    string Name,
    string? Description
);

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
    int Capacity,
    bool IsActive,
    DateTime CreatedAt,
    FacilityTypeDto Type
);

public record FacilityPaginatedDto(
    int TotalPages,
    IEnumerable<FacilityComplexDto> Items
);

public record FacilityCreateDto(
    string Name,
    Guid TypeId,
    int Capacity,
    bool IsActive
);

public record FacilityPatchDto(
    Guid Id,
    string? Name,
    Guid? TypeId,
    int? Capacity,
    bool? IsActive
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

public static class FacilityDtoExtensions
{
    public static FacilityTypeDto ToDto(this FacilityType type)
    {
        return new FacilityTypeDto(
            Id: type.Id,
            Name: type.Name,
            Description: type.Description
        );
    }

    public static FacilityDto ToDto(this Facility facility)
    {
        return new FacilityDto(
            Id: facility.Id,
            Name: facility.Name,
            TypeId: facility.TypeId,
            Capacity: facility.Capacity,
            IsActive: facility.IsActive,
            CreatedAt: facility.CreatedAt
        );
    }

    public static FacilityComplexDto ToComplexDto(this Facility facility)
    {
        return new FacilityComplexDto(
            Id: facility.Id,
            Name: facility.Name,
            Capacity: facility.Capacity,
            IsActive: facility.IsActive,
            CreatedAt: facility.CreatedAt,
            Type: facility.Type.ToDto()
        );
    }
}