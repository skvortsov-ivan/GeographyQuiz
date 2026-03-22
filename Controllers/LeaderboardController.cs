using GeographyQuiz.DTOs;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GeographyQuiz.Controllers
{
    [ApiController]
    [Route("api/leaderboards")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        // Adds a new score to the leaderboard
        // Rate limited with fixed window (10 req/min)
        [HttpPost]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> AddScore([FromBody] LeaderboardRequest request)
        {
            var entry = await _leaderboardService.AddEntryAsync(request.PlayerName, request.Score);
            return Ok(entry);
        }

        // Returns leaderboard entries
        // Rate limited with sliding window (10 req/min)
        [HttpGet]
        [EnableRateLimiting("sliding")]
        public async Task<IActionResult> GetLeaderboard(
            [FromQuery] string? name,
            [FromQuery] DateOnly? date)
        {
            var entries = await _leaderboardService.GetFilteredAsync(name, date);
            return Ok(entries);
        }

        // Updates a player's score (Admin only)
        // Rate limiting disabled for admin endpoints
        [HttpPut("{playerName}")]
        [DisableRateLimiting]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateScore(string playerName, [FromBody] UpdateLeaderboardRequest request)
        {
            var updated = await _leaderboardService.UpdateAsync(playerName, request.Score);
            return Ok(updated);
        }

        // Deletes a leaderboard entry (Admin only)
        // Rate limiting disabled for admin endpoints
        [HttpDelete("{playerName}")]
        [DisableRateLimiting]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteScore(string playerName)
        {
            await _leaderboardService.DeleteAsync(playerName);
            return Ok($"Removed {playerName} from leaderboard.");
        }

        // Returns paginated leaderboard results
        // Rate limited with sliding window (10 req/min)
        [HttpGet("paged")]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var result = await _leaderboardService.GetPagedAsync(page, pageSize);
            return Ok(result);
        }
    }
}