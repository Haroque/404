using SportReservation.Data;
using SportReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace SportReservation.Services;

public class PriceListService(AppDbContext db)
{
    public async Task<List<PriceList>> GetByFacilityType(Guid facilityTypeId, bool onlyActive)
    {
        var now = DateTime.Now;
        var query = db.PriceLists
            .Where(p => p.FacilityTypeId == facilityTypeId);

        if (onlyActive)
        {
            query = query.Where(p => p.ValidFrom <= now && (p.ValidTo == null || p.ValidTo >= now));
        }

        return await query.ToListAsync();
    }

    public async Task<PriceList> Create(PriceListCreateDto dto)
    {
        // Validace logiky a překryvů
        await ValidateOverlap(dto.FacilityTypeId, dto.ValidFrom, dto.ValidTo);
        
        var price = new PriceList
        {
            FacilityTypeId = dto.FacilityTypeId,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            PricePerHour = dto.PricePerHour,
        };

        await db.AddAsync(price);
        await db.SaveChangesAsync();
        return price;
    }
    public async Task<PriceList> Update(Guid id, PriceListUpdateDto dto)
    {
        var price = await db.PriceLists.FindAsync(id) 
                    ?? throw new KeyNotFoundException("Price list were not found");

        // Určení nových časů (pokud v DTO nejsou, zůstávají původní)
        var newFrom = dto.ValidFrom ?? price.ValidFrom;
        var newTo = dto.ValidTo ?? price.ValidTo;

        // byla změna -> validace
        if (dto.ValidFrom != null || dto.ValidTo != null)
        {
            await ValidateOverlap(price.FacilityTypeId, newFrom, newTo, id);
        }

        
        if (dto.PricePerHour.HasValue) price.PricePerHour = dto.PricePerHour.Value; //update
        price.ValidFrom = newFrom;
        price.ValidTo = newTo;

        await db.SaveChangesAsync();
        return price;
    }

    public async Task Delete(Guid id)
    {
        var price = await db.PriceLists.FindAsync(id) 
                    ?? throw new KeyNotFoundException("Price list were not found");
        
        
        //  ValidTo null nelze smazat
        if (price.ValidTo == null || price.ValidTo > DateTime.Now)
        {
            throw new InvalidOperationException("Cannot delete price list");
        }

        db.PriceLists.Remove(price);
        await db.SaveChangesAsync();
    }

    private async Task ValidateOverlap(Guid facilityTypeId, DateTime from, DateTime? to, Guid? excludeId = null)
    {
        if (to != null && from >= to)
        {
            throw new InvalidOperationException("The start of validity must be before the end");
        }
        
        var hasOverlap = await db.PriceLists.AnyAsync(p =>
            p.FacilityTypeId == facilityTypeId &&
            p.Id != excludeId &&
            from < (p.ValidTo ?? DateTime.MaxValue) &&
            (to ?? DateTime.MaxValue) > p.ValidFrom);

        if (hasOverlap)
        {
            throw new InvalidOperationException("The price list overlaps with another existing price list in time");
        }
    }
}