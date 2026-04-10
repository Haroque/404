using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using SportReservation.Data;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation;

public class Cli
{
    public static async Task Run(string[] args, IServiceScope scope)
    {
        Command registerAdmin = new Command("register-admin", "Register admin")
        {
            Arguments =
            {
                new Argument<string>("email"),
                new Argument<string>("fullname"),
                new Argument<string>("password")
            }
        };

        Command seedData = new Command("seed-data", "Seed database with facility types, facilities, and prices");

        Command checkDb = new Command("check-db", "Check database status - shows facilities, types, and prices");

        RootCommand root = new("CLI for administrators");
        root.Subcommands.Add(registerAdmin);
        root.Subcommands.Add(seedData);
        root.Subcommands.Add(checkDb);

        ParseResult parse = root.Parse(args);

        if (parse.Errors.Count != 0)
        {
            foreach (var parseError in parse.Errors)
            {
                await Console.Error.WriteLineAsync(parseError.Message);
            }

            return;
        }

        if (parse.CommandResult.Command.Name == "register-admin")
        {
            var email = parse.GetValue<string>("email")!;
            var fullname = parse.GetValue<string>("fullname")!;
            var password = parse.GetValue<string>("password")!;

            try
            {
                await scope.ServiceProvider.GetRequiredService<UserService>().Register(
                    new RegisterDto(email, fullname, password),
                    UserRole.Admin
                );
            }
            catch (BadHttpRequestException exception)
            {
                Console.WriteLine($"Error when creating admin: {exception.Message}");
                return;
            }

            Console.WriteLine($"Successfully registered admin: {fullname}");
        }
        else if (parse.CommandResult.Command.Name == "seed-data")
        {
            await SeedDatabase(scope);
        }
        else if (parse.CommandResult.Command.Name == "check-db")
        {
            await CheckDatabase(scope);
        }
    }

    private static async Task SeedDatabase(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Check if data already exists
            if (db.FacilityTypes.Any())
            {
                Console.WriteLine("Database already has facility types. Skipping seeding.");
                return;
            }

            Console.WriteLine("Starting database seeding...");

            // Create Facility Types
            var facilityTypes = new[]
            {
                new FacilityType { Id = Guid.NewGuid(), Name = "Tenis", Description = "Tenisové kurty" },
                new FacilityType { Id = Guid.NewGuid(), Name = "Badminton", Description = "Badmintonové haly" },
                new FacilityType { Id = Guid.NewGuid(), Name = "Volejbal", Description = "Volejbalové haly" },
                new FacilityType { Id = Guid.NewGuid(), Name = "Fotbal", Description = "Fotbalová hřiště" }
            };

            foreach (var type in facilityTypes)
            {
                db.FacilityTypes.Add(type);
            }

            await db.SaveChangesAsync();
            Console.WriteLine($"✅ Created {facilityTypes.Length} facility types");

            // Create Facilities for each type
            var facilities = new List<Facility>();

            // Tenis
            var tenis = facilityTypes.First(t => t.Name == "Tenis");
            for (int i = 1; i <= 3; i++)
            {
                facilities.Add(new Facility
                {
                    Id = Guid.NewGuid(),
                    Name = $"Kurt č. {i} - Tenis",
                    TypeId = tenis.Id,
                    Capacity = 4,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            // Badminton
            var badminton = facilityTypes.First(t => t.Name == "Badminton");
            facilities.Add(new Facility
            {
                Id = Guid.NewGuid(),
                Name = "Hala A - Badminton",
                TypeId = badminton.Id,
                Capacity = 8,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            facilities.Add(new Facility
            {
                Id = Guid.NewGuid(),
                Name = "Hala B - Badminton",
                TypeId = badminton.Id,
                Capacity = 8,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            // Volejbal
            var volejbal = facilityTypes.First(t => t.Name == "Volejbal");
            facilities.Add(new Facility
            {
                Id = Guid.NewGuid(),
                Name = "Hala C - Volejbal",
                TypeId = volejbal.Id,
                Capacity = 12,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            // Fotbal
            var fotbal = facilityTypes.First(t => t.Name == "Fotbal");
            facilities.Add(new Facility
            {
                Id = Guid.NewGuid(),
                Name = "Hřiště A - Fotbal",
                TypeId = fotbal.Id,
                Capacity = 22,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            foreach (var facility in facilities)
            {
                db.Facilities.Add(facility);
            }

            await db.SaveChangesAsync();
            Console.WriteLine($"✅ Created {facilities.Count} facilities");

            // Create Price Lists
            var priceLists = new List<PriceList>();
            var today = DateTime.Today;
            var oneYearLater = today.AddYears(1);

            // Tennis prices
            priceLists.Add(new PriceList
            {
                Id = Guid.NewGuid(),
                FacilityTypeId = tenis.Id,
                ValidFrom = today,
                ValidTo = oneYearLater,
                PricePerHour = 300m
            });

            // Badminton prices
            priceLists.Add(new PriceList
            {
                Id = Guid.NewGuid(),
                FacilityTypeId = badminton.Id,
                ValidFrom = today,
                ValidTo = oneYearLater,
                PricePerHour = 250m
            });

            // Volleyball prices
            priceLists.Add(new PriceList
            {
                Id = Guid.NewGuid(),
                FacilityTypeId = volejbal.Id,
                ValidFrom = today,
                ValidTo = oneYearLater,
                PricePerHour = 400m
            });

            // Football prices
            priceLists.Add(new PriceList
            {
                Id = Guid.NewGuid(),
                FacilityTypeId = fotbal.Id,
                ValidFrom = today,
                ValidTo = oneYearLater,
                PricePerHour = 500m
            });

            foreach (var priceList in priceLists)
            {
                db.PriceLists.Add(priceList);
            }

            await db.SaveChangesAsync();
            Console.WriteLine($"✅ Created {priceLists.Count} price lists");

            Console.WriteLine("\n🎉 Database seeding completed successfully!");
            Console.WriteLine("\nCreated:");
            foreach (var type in facilityTypes)
            {
                var typePrice = priceLists.First(p => p.FacilityTypeId == type.Id);
                Console.WriteLine($"  - {type.Name}: {typePrice.PricePerHour} Kč/hod");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error seeding database: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task CheckDatabase(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            Console.WriteLine("\n=== 📊 DATABASE STATUS CHECK ===\n");

            // Check Facility Types
            var facilityTypes = await db.FacilityTypes.ToListAsync();
            Console.WriteLine($"📋 Facility Types: {facilityTypes.Count}");
            if (facilityTypes.Count > 0)
            {
                foreach (var type in facilityTypes)
                {
                    Console.WriteLine($"   ✓ {type.Name} (ID: {type.Id})");
                }
            }
            else
            {
                Console.WriteLine("   ⚠️  No facility types found!");
            }

            // Check Facilities
            var facilities = await db.Facilities.Include(f => f.Type).ToListAsync();
            Console.WriteLine($"\n🏟️  Facilities: {facilities.Count}");
            if (facilities.Count > 0)
            {
                foreach (var facility in facilities)
                {
                    Console.WriteLine($"   ✓ {facility.Name} (Type: {facility.Type?.Name} | IsActive: {facility.IsActive})");
                }
            }
            else
            {
                Console.WriteLine("   ⚠️  No facilities found!");
            }

            // Check Price Lists
            var priceLists = await db.PriceLists.ToListAsync();
            Console.WriteLine($"\n💰 Price Lists: {priceLists.Count}");
            if (priceLists.Count > 0)
            {
                foreach (var price in priceLists)
                {
                    var typeName = facilityTypes.FirstOrDefault(t => t.Id == price.FacilityTypeId)?.Name ?? "Unknown";
                    var today = DateTime.Today;
                    var isActive = price.ValidFrom <= today && (!price.ValidTo.HasValue || price.ValidTo >= today);
                    var activeMarker = isActive ? "✓ ACTIVE" : "⏸️  INACTIVE";
                    Console.WriteLine($"   {activeMarker} | {typeName}: {price.PricePerHour} Kč/hod (Valid: {price.ValidFrom:yyyy-MM-dd} to {(price.ValidTo?.ToString("yyyy-MM-dd") ?? "∞")})");
                }
            }
            else
            {
                Console.WriteLine("   ⚠️  No price lists found!");
            }

            Console.WriteLine("\n=== END OF CHECK ===\n");

            // Final recommendation
            if (facilityTypes.Count == 0 || facilities.Count == 0 || priceLists.Count == 0)
            {
                Console.WriteLine("⚠️  DATABASE IS INCOMPLETE! Run 'dotnet run -- seed-data' to populate it.\n");
            }
            else
            {
                Console.WriteLine("✅ DATABASE LOOKS GOOD!\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error checking database: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}