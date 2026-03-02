namespace DigitalMenuApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ISeedService _seedService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public AdminController(ISeedService seedService, IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _seedService = seedService;
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    /// <summary>
    /// Create initial system admin user (Development only, no auth required)
    /// </summary>
    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<IActionResult> SetupAdmin()
    {
        // Only allow in Development environment
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        // Check if admin already exists
        var existingAdmin = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email == "admin@digitalmenu.com");

        if (existingAdmin != null)
        {
            return Ok(new { message = "Admin user already exists", email = existingAdmin.Email });
        }

        // Create admin user
        var admin = new User
        {
            Email = "admin@digitalmenu.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "System",
            LastName = "Admin",
            RoleId = 1, // system_admin role
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            message = "Admin user created successfully",
            email = "admin@digitalmenu.com",
            password = "Admin123!"
        });
    }

    /// <summary>
    /// Seed sample restaurant data (System Admin only)
    /// Creates 5 restaurant owners with their restaurants, categories, dishes, and ingredients
    /// </summary>
    [HttpPost("seed")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> SeedSampleData()
    {
        var result = await _seedService.SeedSampleDataAsync();

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }
}
