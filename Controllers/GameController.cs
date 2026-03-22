using GeographyQuiz.DTOs;
using GeographyQuiz.Models;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/games")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    // Returns a new round with two countries
    // Rate limited with sliding window (10 req/min)
    [HttpGet("round")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<CountryRoundResponse>> GetRound()
    {
        var round = await _gameService.GenerateRoundAsync();
        return Ok(round);
    }

    // Evaluates the player's answer for the current round
    // Rate limited with fixed window (10 req/min)
    [HttpPost("answer")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> SubmitAnswer([FromQuery] string selected)
    {
        var result = await _gameService.EvaluateAnswer(selected);

        return Ok(new
        {
            correct = result.isCorrect,
            winner = result.winner
        });
    }

    // Resets all game state and starts a new game
    // Rate limited with fixed window (10 req/min)
    [HttpPost("new")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> NewGame()
    {
        await _gameService.ResetGame();
        return Ok("New game started.");
    }
}
