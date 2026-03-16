namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net;
using System.Security.Cryptography;
using DigitalMenuApi.Helpers;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for {Email}", request.Email);

        //1. Check if email exists
        if (await _unitOfWork.Users.Query().AsNoTracking().AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            return Result<AuthResponse>.Failure("Email already registered", 400);
        }

        //2. Get RoleId
        if (request.AccountType == "system_admin")
        {
            _logger.LogWarning("Registration failed: Attempted system_admin registration for {Email}", request.Email);
            return Result<AuthResponse>.Failure("Invalid account type", 400);
        }
        var role = await _unitOfWork.Roles.Query().AsNoTracking().FirstOrDefaultAsync(r => r.Name == request.AccountType);
        if (role == null)
        {
            _logger.LogWarning("Registration failed: Invalid account type {AccountType}", request.AccountType);
            return Result<AuthResponse>.Failure("Invalid account type", 400);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            //3. Create User
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.HashPassword(request.Password),
                RoleId = role.Id,
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            //4. Generate token and return response
            var responseData = await GenerateAuthResponse(user, role.Name);

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("User registered successfully: {UserId} ({Email})", user.Id, user.Email);
            return Result<AuthResponse>.Success(responseData);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Registration failed unexpected for {Email}", request.Email);
            return Result<AuthResponse>.Failure("Registration failed due to a server error", 500);
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);

        //1. Check if email exists
        var user = await _unitOfWork.Users
        .Query()
        .AsNoTracking()
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null)
        {
            _logger.LogWarning("Login failed: Email {Email} not found", request.Email);
            return Result<AuthResponse>.Failure("Invalid email or password", 400);
        }

        //2. Verify password
        if (!BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
            return Result<AuthResponse>.Failure("Invalid email or password", 400);
        }

        //3. Check active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: Account deactivated for {Email}", request.Email);
            return Result<AuthResponse>.Failure("Account is deactivated", 400);
        }

        _logger.LogInformation("User logged in: {UserId} ({Email})", user.Id, user.Email);

        //4. Generate token and return response
        return Result<AuthResponse>.Success(await GenerateAuthResponse(user, user.Role.Name));
    }

    public async Task<Result<bool>> IsEmailExistsAsync(string email)
    {
        return Result<bool>.Success(await _unitOfWork.Users.Query().AsNoTracking().AnyAsync(u => u.Email == email));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogDebug("Token refresh attempt");

        //1. Find the refresh token in database
        var storedToken = await _unitOfWork.RefreshTokens
            .Query()
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Token refresh failed: Invalid token");
            return Result<AuthResponse>.Failure("Invalid refresh token", 400);
        }

        //2. Check if the token is active
        if (!storedToken.IsActive)
        {
            //Security: someone is trying to use a revoked or expired token, revoke all user's tokens
            if (storedToken.IsRevoked)
            {
                _logger.LogError("SECURITY: Attempted reuse of revoked token for UserId {UserId}", storedToken.UserId);
                await RevokeAllUserTokensAsync(storedToken.UserId, "Attempted reuse of revoked token");
            }
            else
            {
                _logger.LogWarning("Token refresh failed: Token expired for UserId {UserId}", storedToken.UserId);
            }

            return Result<AuthResponse>.Failure("Attempted reuse of revoked token", 400);
        }

        //3. Check user is still active
        if (!storedToken.User.IsActive)
        {
            _logger.LogWarning("Token refresh failed: Account deactivated for UserId {UserId}", storedToken.UserId);
            return Result<AuthResponse>.Failure("Account is deactivated", 400);
        }

        //4. Rotate the token
        storedToken.RevokedAt = DateTime.UtcNow;
        var newRefreshToken = await CreateRefreshTokenAsync(storedToken.UserId);
        storedToken.ReplacedByToken = newRefreshToken.Token;

        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token refreshed for UserId {UserId}", storedToken.UserId);

        //5. Generate new access token
        var user = storedToken.User;
        var accessToken = GenerateJwtToken(user, user.Role.Name);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expireMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

        return Result<AuthResponse>.Success(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.Name,
            Token = accessToken,
            RefreshToken = newRefreshToken.Token,
            TokenExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
            RefreshTokenExpiresAt = newRefreshToken.ExpiresAt
        });
    }

    public async Task<Result<bool>> RevokeTokenAsync(string token)
    {
        _logger.LogDebug("Token revoke attempt");

        //1. Check valid token
        var storedToken = await _unitOfWork.RefreshTokens
            .Query()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (storedToken == null)
        {
            _logger.LogWarning("Token revoke failed: Invalid token");
            return Result<bool>.Failure("Invalid token", 400);
        }

        //2. check if already revoked
        if (storedToken.RevokedAt != null)
        {
            _logger.LogWarning("Token revoke failed: Already revoked for UserId {UserId}", storedToken.UserId);
            return Result<bool>.Failure("Token already revoked", 400);
        }

        //3. Revoke
        storedToken.RevokedAt = DateTime.UtcNow;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token revoked for UserId {UserId}", storedToken.UserId);
        return Result<bool>.Success(true);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user, string roleName)
    {
        var token = GenerateJwtToken(user, roleName);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expireMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roleName,
            Token = token,
            RefreshToken = refreshToken.Token,
            TokenExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        };
    }

    private string GenerateJwtToken(User user, string roleName)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new Exception("JwtSettings:SecretKey is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpiresDays = int.Parse(jwtSettings["RefreshTokenExpiresDays"] ?? "7");
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateSecureRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresDays)
        };
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();
        return refreshToken;
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task RevokeAllUserTokensAsync(int userId, string reason)
    {
        var activeTokens = await _unitOfWork.RefreshTokens
            .Query()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(token);
        }
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("SECURITY: All tokens revoked for UserId {UserId}. Reason: {Reason}. Count: {Count}",
            userId, reason, activeTokens.Count);
    }
}