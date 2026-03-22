using GeographyQuiz.DTOs;
using GeographyQuiz.Models;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

//Routes ska alltid vara i plural
[Route("api/games")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("round")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<CountryRoundResponse>> GetRound()
    {
        var round = await _gameService.GenerateRoundAsync();
        return Ok(round);
    }

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

    [HttpPost("new")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> NewGame()
    {
        await _gameService.ResetGame();
        return Ok("New game started.");
    }

}
