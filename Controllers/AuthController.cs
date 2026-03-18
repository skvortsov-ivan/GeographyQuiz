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

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validera användaren (I en riktig app kollar vi mot databasen)
            if (request.Username != "admin" || request.Password != "123")
            {
                return Unauthorized("Felaktigt användarnamn eller lösenord.");
            }

            // Skapa säkerhetsnyckeln utifrån vår hemlighet i appsettings.json
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(ClaimTypes.Role, "Admin")};

            // Skapa själva token-objektet med vår konfiguration
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            // Returnera token som en sträng
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }
    }

    // En enkel DTO för att ta emot datan
    public record LoginRequest(string Username, string Password);
}
