using SportReservation.Data;
using SportReservation.Models;

namespace SportReservation.Services;

public class PriceListService(AppDbContext db)
{
    public async Task<PriceList> Create(PriceListCreateDto dto)
    {
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
}