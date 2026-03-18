using GeographyQuiz.DTOs;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeographyQuiz.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpPost]
        public IActionResult AddScore([FromBody] LeaderboardRequest request)
        {
            var entry = _leaderboardService.AddEntry(request.PlayerName, request.Score);
            return Ok(entry);
        }

        [HttpGet]
        public IActionResult GetLeaderboard(
            [FromQuery] string? name,
            [FromQuery] DateOnly? date)
        {
            var entries = _leaderboardService.GetFiltered(name, date);
            return Ok(entries);
        }


        [HttpPut("{playerName}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateScore([FromBody] UpdateLeaderboardRequest request)
        {
            var updated = _leaderboardService.Update(request.PlayerName, request.Score);
            return Ok(updated);
        }

        [HttpDelete("{playerName}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteScore(string playerName)
        {
            _leaderboardService.Delete(playerName);
            return Ok($"Removed {playerName} from leaderboard.");
        }
    }
}

