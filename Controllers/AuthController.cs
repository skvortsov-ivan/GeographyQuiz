using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GeographyQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // Authenticates a user and returns a JWT token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Basic validation (in a real app, validate against a database)
            if (request.Username != "admin" || request.Password != "123")
            {
                return Unauthorized("Felaktigt användarnamn eller lösenord.");
            }

            // Create signing key from appsettings.json secret
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims included in the token (username + admin role)
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            // Convert token object to string and return token
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }
    }

    // DTO for login credentials
    public record LoginRequest(string Username, string Password);
}
