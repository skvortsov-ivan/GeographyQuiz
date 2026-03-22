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

        [HttpPost]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> AddScore([FromBody] LeaderboardRequest request)
        {
            var entry = await _leaderboardService.AddEntryAsync(request.PlayerName, request.Score);
            return Ok(entry);
        }

        [HttpGet]
        [EnableRateLimiting("sliding")]
        public async Task<IActionResult> GetLeaderboard(
            [FromQuery] string? name,
            [FromQuery] DateOnly? date)
        {
            var entries = await _leaderboardService.GetFilteredAsync(name, date);
            return Ok(entries);
        }


        [HttpPut("{playerName}")]
        [DisableRateLimiting]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateScore(string playerName, [FromBody] UpdateLeaderboardRequest request)
        {
            var updated = await _leaderboardService.UpdateAsync(playerName, request.Score);
            return Ok(updated);
        }

        [HttpDelete("{playerName}")]
        [DisableRateLimiting]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteScore(string playerName)
        {
            await _leaderboardService.DeleteAsync(playerName);
            return Ok($"Removed {playerName} from leaderboard.");
        }

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