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

        public AuthController(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Authentication newUser)
        {
            _logger.LogInformation("Register endpoint called");
            _logger.LogInformation("Email: {Email}", newUser.Email);

            var existingUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Email already in use: {Email}", newUser.Email);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return Problem("An error occurred during registration.");
            }
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Authentication currentUser)
        {
            _logger.LogInformation("Login endpoint called");
            _logger.LogInformation("Email: {Email}", currentUser.Email);

            var user = await _userRepository.GetUserByEmailAsync(currentUser.Email);
            if (user == null || !VerifyPassword(currentUser.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid email or password for: {Email}", currentUser.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = GenerateJwtToken(user);
            _logger.LogInformation("User logged in successfully: {Email}", currentUser.Email);
            return Ok(new { access_token = token });
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}