using Microsoft.EntityFrameworkCore;
using SportReservation.Data;
using SportReservation.Models;

namespace SportReservation.Services;

public class FacilityService(AppDbContext db)
{
    public async Task<FacilityPaginatedDto> GetPagedAsync(
        int page,
        int perPage,
        Guid? typeId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        if (page < 1)
            page = 1;

        if (perPage < 1)
            perPage = 10;

        var query = db.Facilities
            .Include(f => f.Type)
            .AsQueryable();

        if (typeId.HasValue)
        {
            query = query.Where(f => f.TypeId == typeId.Value);
        }

        if (from.HasValue && to.HasValue)
        {
            var fromValue = from.Value;
            var toValue = to.Value;

            query = query.Where(f =>
                f.IsActive &&
                !db.Downtimes.Any(d =>
                    d.FacilityId == f.Id &&
                    d.StartAt < toValue &&
                    d.EndAt > fromValue) &&
                !db.Reservations.Any(r =>
                    r.FacilityId == f.Id &&
                    r.Status == ReservationStatus.Active &&
                    r.StartAt < toValue &&
                    r.EndAt > fromValue));
        }

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)perPage);

        var facilities = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return new FacilityPaginatedDto(
            TotalPages: totalPages,
            Items: facilities.Select(f => f.ToComplexDto())
        );
    }

    public async Task<Facility?> GetAsync(Guid id)
    {
        return await db.Facilities
            .Include(f => f.Type)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FacilityComplexDto?> CreateAsync(FacilityCreateDto dto)
    {
        var type = await db.FacilityTypes.FirstOrDefaultAsync(t => t.Id == dto.TypeId);

        if (type == null)
            return null;

        var facility = new Facility
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            TypeId = dto.TypeId,
            Capacity = dto.Capacity,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            Type = type
        };

        db.Facilities.Add(facility);
        await db.SaveChangesAsync();

        return facility.ToComplexDto();
    }

    public async Task<FacilityComplexDto?> PatchAsync(FacilityPatchDto dto)
    {
        var facility = await db.Facilities
            .Include(f => f.Type)
            .FirstOrDefaultAsync(f => f.Id == dto.Id);

        if (facility == null)
            return null;

        if (dto.TypeId.HasValue && dto.TypeId.Value != facility.TypeId)
        {
            var newType = await db.FacilityTypes.FirstOrDefaultAsync(t => t.Id == dto.TypeId.Value);

            if (newType == null)
                return null;

            facility.TypeId = newType.Id;
            facility.Type = newType;
        }

        if (dto.Name is not null)
            facility.Name = dto.Name;

        if (dto.Capacity.HasValue)
            facility.Capacity = dto.Capacity.Value;

        if (dto.IsActive.HasValue)
            facility.IsActive = dto.IsActive.Value;

        await db.SaveChangesAsync();

        return facility.ToComplexDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var facility = await db.Facilities.FirstOrDefaultAsync(f => f.Id == id);

        if (facility == null)
            return false;

        db.Facilities.Remove(facility);

        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<IEnumerable<FacilityTypeDto>> GetTypesAsync()
    {
        return await db.FacilityTypes
            .OrderBy(t => t.Name)
            .Select(t => new FacilityTypeDto(
                t.Id,
                t.Name,
                t.Description
            ))
            .ToListAsync();
    }

    public async Task<FacilityTypeDto> CreateTypeAsync(FacilityTypeCreateDto dto)
    {
        var type = new FacilityType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        db.FacilityTypes.Add(type);
        await db.SaveChangesAsync();

        return type.ToDto();
    }

    public async Task<FacilityTypeDto?> PatchTypeAsync(FacilityTypePatchDto dto)
    {
        var type = await db.FacilityTypes.FirstOrDefaultAsync(t => t.Id == dto.Id);

        if (type == null)
            return null;

        if (dto.Name is not null)
            type.Name = dto.Name;

        if (dto.Description is not null)
            type.Description = dto.Description;

        await db.SaveChangesAsync();

        return type.ToDto();
    }

    public async Task<bool> DeleteTypeAsync(Guid id)
    {
        var type = await db.FacilityTypes.FirstOrDefaultAsync(t => t.Id == id);

        if (type == null)
            return false;

        db.FacilityTypes.Remove(type);

        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}