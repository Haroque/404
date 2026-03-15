using SportReservation.Data;
using SportReservation.Models;

namespace SportReservation.Services;

public class FacilityService(AppDbContext db)
{
    public async Task<Facility> Create(FacilityCreateDto dto)
    {
        var facility = new Facility
        {
            Name = dto.Name,
            TypeId = dto.TypeId,
            Capacity = dto.Capacity,
            IsActive = dto.IsActive
        };

        await db.AddAsync(facility);
        await db.SaveChangesAsync();
        return facility;
    }

    public async Task<FacilityType> CreateType(FacilityTypeCreateDto dto)
    {
        var type = new FacilityType
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await db.AddAsync(type);
        await db.SaveChangesAsync();
        return type;
    }
}