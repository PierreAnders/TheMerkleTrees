using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using Microsoft.Extensions.Logging;

namespace TheMerkleTrees.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;

            // Chargement sécurisé des paramètres JWT
            _jwtSettings = new JwtSettings
            {
                Key = Environment.GetEnvironmentVariable("JWT_KEY") ?? configuration["Jwt:Key"],
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? configuration["Jwt:Issuer"],
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? configuration["Jwt:Audience"],
                ExpirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 120
            };

            ValidateJwtSettings();
        }

        private void ValidateJwtSettings()
        {
            if (string.IsNullOrEmpty(_jwtSettings.Key) || _jwtSettings.Key.Length < 32)
                throw new ArgumentException("JWT Key must be at least 32 characters long");

            if (string.IsNullOrEmpty(_jwtSettings.Issuer))
                throw new ArgumentException("JWT Issuer is required");

            if (string.IsNullOrEmpty(_jwtSettings.Audience))
                throw new ArgumentException("JWT Audience is required");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Authentication newUser)
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", newUser.Email);

            var existingUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - Email already in use: {Email}", newUser.Email);
                return BadRequest(new { message = "Email already in use" });
            }

            var user = new User
            {
                Email = newUser.Email,
                PasswordHash = HashPassword(newUser.Password)
            };

            try
            {
                await _userRepository.CreateUserAsync(user);
                _logger.LogInformation("User registered successfully: {Email}", newUser.Email);
                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", newUser.Email);
                return Problem("An error occurred during registration.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Authentication currentUser)
        {
            _logger.LogInformation("Login attempt for email: {Email}", currentUser.Email);

            var user = await _userRepository.GetUserByEmailAsync(currentUser.Email);
            if (user == null || !VerifyPassword(currentUser.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", currentUser.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = GenerateJwtToken(user);
            _logger.LogInformation("Successful login for email: {Email}", currentUser.Email);
            
            return Ok(new { 
                access_token = token,
                expires_in = _jwtSettings.ExpirationMinutes * 60
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hash = HashPassword(password);
            return hash == hashedPassword;
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private class JwtSettings
        {
            public string Key { get; set; } = string.Empty;
            public string Issuer { get; set; } = string.Empty;
            public string Audience { get; set; } = string.Empty;
            public int ExpirationMinutes { get; set; }
        }
    }
}
