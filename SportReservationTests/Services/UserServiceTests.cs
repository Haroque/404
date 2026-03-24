using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MockQueryable.FakeItEasy;
using SportReservation.Data;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservationTests.Services;

public class UserServiceTests
{
    private readonly List<User> _users =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Email = "sample@example.com",
            FullName = "Sample User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = UserRole.User
        }
    ];

    private readonly UserService _service;

    public UserServiceTests()
    {
        AppDbContext db = A.Fake<AppDbContext>(options => options
            .WithArgumentsForConstructor([new DbContextOptions<AppDbContext>()])
        );
        A.CallTo(() => db.AddAsync(A<User>._, A<CancellationToken>._))
            .Invokes((User user, CancellationToken _) => _users.Add(user));

        A.CallTo(() => db.Users)
            .Returns(_users.BuildMockDbSet());

        _service = new UserService(db);
    }

    [Fact]
    public async Task RegisterUser_Success()
    {
        var user = await _service.Register(
            new RegisterDto("test@gmail.com", "Test", "password"),
            UserRole.User
        );
        Assert.Equal("Test", user.FullName);
    }

    [Fact]
    public async Task RegisterUser_Duplicate()
    {
        await _service.Register(
            new RegisterDto("test@gmail.com", "Test", "password"),
            UserRole.User
        );

        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.Register(
            new RegisterDto("test@gmail.com", "Test", "password"),
            UserRole.User
        ));

        Assert.Equal(400, exception.StatusCode);
    }
}