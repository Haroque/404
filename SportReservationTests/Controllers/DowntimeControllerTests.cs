using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.FakeItEasy;
using SportReservation.Controllers;
using SportReservation.Data;
using SportReservation.Models;

namespace SportReservationTests.Controllers;

public class DowntimeControllerTests
{
    private readonly List<Facility> _facilities;
    private readonly List<Downtime> _downtimes;

    private readonly AppDbContext _db;
    private readonly DbSet<Facility> _facilitiesDbSet;
    private readonly DbSet<Downtime> _downtimesDbSet;

    public DowntimeControllerTests()
    {
        var activeFacilityId = Guid.NewGuid();
        var secondFacilityId = Guid.NewGuid();

        _facilities =
        [
            new Facility
            {
                Id = activeFacilityId,
                Name = "Court A",
                TypeId = Guid.NewGuid(),
                Capacity = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Facility
            {
                Id = secondFacilityId,
                Name = "Court B",
                TypeId = Guid.NewGuid(),
                Capacity = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        ];

        _downtimes =
        [
            new Downtime
            {
                Id = Guid.NewGuid(),
                FacilityId = activeFacilityId,
                StartAt = DateTime.UtcNow.AddHours(-1),
                EndAt = DateTime.UtcNow.AddHours(2),
                Reason = "Cleaning"
            },
            new Downtime
            {
                Id = Guid.NewGuid(),
                FacilityId = activeFacilityId,
                StartAt = DateTime.UtcNow.AddHours(5),
                EndAt = DateTime.UtcNow.AddHours(7),
                Reason = "Maintenance"
            },
            new Downtime
            {
                Id = Guid.NewGuid(),
                FacilityId = activeFacilityId,
                StartAt = DateTime.UtcNow.AddDays(-3),
                EndAt = DateTime.UtcNow.AddDays(-2),
                Reason = "Old downtime"
            },
            new Downtime
            {
                Id = Guid.NewGuid(),
                FacilityId = secondFacilityId,
                StartAt = DateTime.UtcNow.AddHours(1),
                EndAt = DateTime.UtcNow.AddHours(3),
                Reason = "Other facility"
            }
        ];

        _db = A.Fake<AppDbContext>(options => options
            .WithArgumentsForConstructor([new DbContextOptions<AppDbContext>()])
        );

        _facilitiesDbSet = _facilities.BuildMockDbSet();
        _downtimesDbSet = _downtimes.BuildMockDbSet();

        A.CallTo(() => _db.Facilities).Returns(_facilitiesDbSet);
        A.CallTo(() => _db.Downtimes).Returns(_downtimesDbSet);

        A.CallTo(() => _db.SaveChangesAsync(A<CancellationToken>._))
            .Returns(1);

        A.CallTo(() => _facilitiesDbSet.FindAsync(A<object[]>._))
            .ReturnsLazily((object[] ids) =>
            {
                var id = (Guid)ids[0];
                var facility = _facilities.FirstOrDefault(f => f.Id == id);
                return new ValueTask<Facility?>(facility);
            });

        A.CallTo(() => _downtimesDbSet.FindAsync(A<object[]>._))
            .ReturnsLazily((object[] ids) =>
            {
                var id = (Guid)ids[0];
                var downtime = _downtimes.FirstOrDefault(d => d.Id == id);
                return new ValueTask<Downtime?>(downtime);
            });

        A.CallTo(() => _downtimesDbSet.AddAsync(A<Downtime>._, A<CancellationToken>._))
            .Invokes((Downtime downtime, CancellationToken _) => _downtimes.Add(downtime))
            .ReturnsLazily((Downtime _, CancellationToken _) =>
                default(ValueTask<EntityEntry<Downtime>>));

        A.CallTo(() => _downtimesDbSet.Update(A<Downtime>._))
            .ReturnsLazily((Downtime downtime) =>
            {
                var index = _downtimes.FindIndex(d => d.Id == downtime.Id);
                if (index >= 0)
                {
                    _downtimes[index] = downtime;
                }

                return null!;
            });

        A.CallTo(() => _downtimesDbSet.Remove(A<Downtime>._))
            .ReturnsLazily((Downtime downtime) =>
            {
                _downtimes.Remove(downtime);
                return null!;
            });
    }

    private static DowntimeController CreateController(UserRole role, AppDbContext db)
    {
        var controller = new DowntimeController(db);
        var httpContext = new DefaultHttpContext();

        httpContext.Items["User"] = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash",
            Role = role
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task GetForFacility_ReturnsOnlyCurrentAndFutureForGivenFacility()
    {
        var controller = CreateController(UserRole.User, _db);
        var facilityId = _facilities[0].Id;

        var result = await controller.GetForFacility(facilityId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<DowntimeDto>>(ok.Value);
        var list = items.ToList();

        Assert.Equal(2, list.Count);
        Assert.All(list, d => Assert.Equal(facilityId, d.FacilityId));
        Assert.DoesNotContain(list, d => d.Reason == "Old downtime");
        Assert.Equal("Cleaning", list[0].Reason);
        Assert.Equal("Maintenance", list[1].Reason);
    }

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ReturnsOkAndAddsDowntime()
    {
        var controller = CreateController(UserRole.Admin, _db);
        var facilityId = _facilities[0].Id;

        var body = new CreateDowntimeDto(
            facilityId,
            DateTime.UtcNow.AddHours(10),
            DateTime.UtcNow.AddHours(12),
            "Tournament prep"
        );

        var result = await controller.Create(body);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<DowntimeDto>(ok.Value);

        Assert.Equal(facilityId, dto.FacilityId);
        Assert.Equal("Tournament prep", dto.Reason);
        Assert.Contains(_downtimes, d => d.Id == dto.Id && d.Reason == "Tournament prep");
    }

    [Fact]
    public async Task Create_AsNonAdmin_ReturnsForbidden()
    {
        var controller = CreateController(UserRole.User, _db);
        var facilityId = _facilities[0].Id;

        var body = new CreateDowntimeDto(
            facilityId,
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Blocked"
        );

        var result = await controller.Create(body);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Equal("forbidden", forbidden.Value);
    }

    [Fact]
    public async Task Create_WhenStartIsAfterEnd_ReturnsBadRequest()
    {
        var controller = CreateController(UserRole.Admin, _db);
        var facilityId = _facilities[0].Id;

        var body = new CreateDowntimeDto(
            facilityId,
            DateTime.UtcNow.AddHours(5),
            DateTime.UtcNow.AddHours(2),
            "Invalid interval"
        );

        var result = await controller.Create(body);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("start-after-end", badRequest.Value);
    }

    [Fact]
    public async Task Create_WhenFacilityDoesNotExist_ReturnsNotFound()
    {
        var controller = CreateController(UserRole.Admin, _db);

        var body = new CreateDowntimeDto(
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Unknown facility"
        );

        var result = await controller.Create(body);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("facility-not-found", notFound.Value);
    }

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ReturnsOkAndUpdatesDowntime()
    {
        var controller = CreateController(UserRole.Admin, _db);
        var existing = _downtimes[0];
        var newFacilityId = _facilities[1].Id;

        var body = new UpdateDowntimeDto(
            newFacilityId,
            DateTime.UtcNow.AddHours(20),
            DateTime.UtcNow.AddHours(22),
            "Updated reason"
        );

        var result = await controller.Update(existing.Id, body);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<DowntimeDto>(ok.Value);

        Assert.Equal(existing.Id, dto.Id);
        Assert.Equal(newFacilityId, dto.FacilityId);
        Assert.Equal("Updated reason", dto.Reason);

        var updated = _downtimes.First(d => d.Id == existing.Id);
        Assert.Equal(newFacilityId, updated.FacilityId);
        Assert.Equal("Updated reason", updated.Reason);
    }

    [Fact]
    public async Task Update_WhenDowntimeDoesNotExist_ReturnsNotFound()
    {
        var controller = CreateController(UserRole.Admin, _db);
        var body = new UpdateDowntimeDto(
            _facilities[0].Id,
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Missing"
        );

        var result = await controller.Update(Guid.NewGuid(), body);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_WhenFacilityDoesNotExist_ReturnsNotFound()
    {
        var controller = CreateController(UserRole.Admin, _db);
        var existing = _downtimes[0];

        var body = new UpdateDowntimeDto(
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Missing facility"
        );

        var result = await controller.Update(existing.Id, body);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("facility-not-found", notFound.Value);
    }

    [Fact]
    public async Task Delete_AsAdmin_RemovesDowntimeAndReturnsNoContent()
    {
        var controller = CreateController(UserRole.Admin, _db);
        var existing = _downtimes[0];

        var result = await controller.Delete(existing.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.DoesNotContain(_downtimes, d => d.Id == existing.Id);
    }

    [Fact]
    public async Task Delete_AsNonAdmin_ThrowsBadHttpRequestException()
    {
        var controller = CreateController(UserRole.User, _db);
        var existing = _downtimes[0];

        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await controller.Delete(existing.Id)
        );

        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
        Assert.Equal("forbidden", exception.Message);
    }

    [Fact]
    public async Task Delete_WhenDowntimeDoesNotExist_ReturnsNotFound()
    {
        var controller = CreateController(UserRole.Admin, _db);

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}