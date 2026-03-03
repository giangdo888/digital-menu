using DigitalMenuApi.Data;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Implementations;
using DigitalMenuApi.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DigitalMenuApi.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    public AuthServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        // Setup mock JWT config so GenerateJwtToken doesn't throw
        var jwtSettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "SuperSecretKeyForTestingThatIsAtLeast32Bytes!"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpiryInMinutes", "60"},
            {"JwtSettings:RefreshTokenExpiresDays", "7"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtSettings)
            .Build();

        _mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns((string key) => configuration.GetSection(key));
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var context = new ApplicationDbContext(options);

        // Seed basic roles needed for auth
        context.Roles.AddRange(
            new Role { Id = 1, Name = "system_admin", Description = "System Admin" },
            new Role { Id = 2, Name = "restaurant_admin", Description = "Restaurant Admin" },
            new Role { Id = 3, Name = "customer", Description = "Customer" }
        );
        context.SaveChanges();

        return new UnitOfWork(context);
    }

    // We want to make sure people can't register with an email that's already in the system.
    // If they try, the service should immediately flag it as a failure.
    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        // Seed an existing user
        await uow.Users.AddAsync(new User 
        { 
            Email = "test@example.com", 
            FirstName = "Test", 
            LastName = "User", 
            PasswordHash = "hash", 
            RoleId = 3, 
            IsActive = true 
        });
        await uow.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User",
            AccountType = "customer"
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email already registered");
    }

    // For security reasons, regular users signing up from the website or app
    // shouldn't be able to just pass in "system_admin" as their account type.
    // This test ensures the system blocks that attempt.
    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenAccountTypeIsSystemAdmin()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "Admin",
            AccountType = "system_admin" // Not allowed via generic registration
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid account type");
    }

    // This is the happy path! When a normal customer registers with valid details,
    // we want to ensure their account is created, they get a token back, and, most importantly,
    // their password is securely hashed in the database before saving.
    [Fact]
    public async Task RegisterAsync_ShouldSucceed_AndHashPassword_WhenValid()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "Customer",
            AccountType = "customer"
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be(request.Email);
        result.Data.Token.Should().NotBeNullOrEmpty();

        // Verify database state
        var savedUser = await uow.Users.Query().FirstOrDefaultAsync(u => u.Email == request.Email);
        savedUser.Should().NotBeNull();
        savedUser.PasswordHash.Should().NotBe(request.Password); // Ensure it's hashed
        BCrypt.Net.BCrypt.Verify(request.Password, savedUser.PasswordHash).Should().BeTrue();
    }

    // If someone tries to log in with an email that we don't have on file,
    // we just want to reject it and tell them the credentials aren't right.
    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        var request = new LoginRequest { Email = "ghost@example.com", Password = "pwd" };

        // Act
        var result = await sut.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid email or password");
    }

    // If they get the email right but type the wrong password,
    // the BCrypt password verification should fail and reject the login attempt.
    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIncorrect()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        await uow.Users.AddAsync(new User 
        { 
            Email = "test@example.com", 
            FirstName = "Test", 
            LastName = "User", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"), 
            RoleId = 3, 
            IsActive = true 
        });
        await uow.SaveChangesAsync();

        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword!" };

        // Act
        var result = await sut.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid email or password");
    }

    // Sometimes admins might deactivate a user (maybe they broke the rules).
    // Even if their email and password are correct, the system should check their
    // active status and block them from getting a new login token.
    [Fact]
    public async Task LoginAsync_ShouldFail_WhenAccountIsDeactivated()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        await uow.Users.AddAsync(new User 
        { 
            Email = "test@example.com", 
            FirstName = "Test", 
            LastName = "User", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"), 
            RoleId = 3, 
            IsActive = false // Deactivated
        });
        await uow.SaveChangesAsync();

        var request = new LoginRequest { Email = "test@example.com", Password = "CorrectPassword!" };

        // Act
        var result = await sut.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account is deactivated");
    }

    // The happy path for logging in! If the email, password, and active status
    // are all good, we expect them to get a fresh JWT token to use the app.
    [Fact]
    public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreValid()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new AuthService(uow, _mockConfig.Object, _mockLogger.Object);

        await uow.Users.AddAsync(new User 
        { 
            Email = "test@example.com", 
            FirstName = "Test", 
            LastName = "User", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"), 
            RoleId = 3, 
            IsActive = true 
        });
        await uow.SaveChangesAsync();

        var request = new LoginRequest { Email = "test@example.com", Password = "CorrectPassword!" };

        // Act
        var result = await sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }
}
