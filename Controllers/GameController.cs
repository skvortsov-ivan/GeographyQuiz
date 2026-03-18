using GeographyQuiz.DTOs;
using GeographyQuiz.Models;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/game")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("round")]
    public async Task<ActionResult<CountryRoundResponse>> GetRound()
    {
        var round = await _gameService.GenerateRoundAsync();
        return Ok(round);
    }

    [HttpPost("answer")]
    public IActionResult SubmitAnswer([FromQuery] string selected)
    {
        var result = _gameService.EvaluateAnswer(selected);

        return Ok(new
        {
            correct = result.isCorrect,
            winner = result.winner
        });
    }

    [HttpPost("new")]
    public IActionResult NewGame()
    {
        _gameService.ResetGame();
        return Ok("New game started.");
    }
}
