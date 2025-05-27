using EventTestTask.Core.Entities;
using EventTestTask.Core.Enums;
using EventTestTask.Infrastructure.ApplicationContext;
using EventTestTask.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventTestTask.Tests.User;

public class UserRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public UserRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AppDbContext CreateDbContext()
    {
        return new AppDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserWithEvents()
    {
        var userId = Guid.NewGuid();

        var user = new Core.Entities.User(
            id: userId,
            firstName: "Test1",
            lastName: "Test2",
            birthDate: DateTime.UtcNow.AddYears(2),
            email: "test@example.com",
            passwordHash: "passwordTest",
            role: UserRole.User
        );

        var events = new List<Event>
        {
            new Event(
                id: Guid.NewGuid(),
                title: "Event 1",
                description: "Description 1",
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddHours(2),
                location: "Location 1",
                category: EventCategory.Conference,
                maxParticipants: 10,
                image: Array.Empty<byte>()
            ),
            new Event(
                id: Guid.NewGuid(),
                title: "Event 2",
                description: "Description 2",
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddHours(3),
                location: "Location 2",
                category: EventCategory.Workshop,
                maxParticipants: 5,
                image: Array.Empty<byte>()
            )
        };

        var registrations = events.Select(e => new Registration(
            id: Guid.NewGuid(),
            userId: userId,
            eventId: e.Id,
            registrationDate: DateTime.UtcNow
        )).ToList();

        await using (var context = CreateDbContext())
        {
            context.Users.Add(user);
            context.Events.AddRange(events);
            context.Registrations.AddRange(registrations);
            await context.SaveChangesAsync();
        }

        Core.Entities.User? result;
        await using (var context = CreateDbContext())
        {
            var repository = new UsersRepository(context);
            result = await repository.GetUserByIdAsync(userId, CancellationToken.None);
        }

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Events.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var existingUserId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();

        var user = new Core.Entities.User(
            id: existingUserId,
            firstName: "Test1",
            lastName: "Test2",
            birthDate: DateTime.UtcNow.AddYears(-20),
            email: "test@example.com",
            passwordHash: "passwordTest",
            role: UserRole.User
        );

        await using (var context = CreateDbContext())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        
        await using (var context = CreateDbContext())
        {
            var repository = new UsersRepository(context);

            var result = await repository.GetUserByIdAsync(nonExistentUserId, CancellationToken.None);
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUserInDatabase()
    {
        var user = new Core.Entities.User(
            Guid.Empty,
            "John",
            "Doe",
            new DateTime(1990, 1, 1),
            "john.doe@example.com",
            "hashed_password",
            UserRole.User
        );

        await using var context = CreateDbContext();
        var repository = new UsersRepository(context);

        await repository.CreateUserAsync(user, CancellationToken.None);

        var createdUser = await context.Users.FirstOrDefaultAsync();
        createdUser.Should().NotBeNull();
        createdUser!.Id.Should().NotBe(Guid.Empty);
        createdUser.FirstName.Should().Be("John");
        createdUser.Email.Should().Be("john.doe@example.com");
        createdUser.PasswordHash.Should().Be("hashed_password");
    }
}