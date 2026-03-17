using GeographyQuiz.DTOs;
using GeographyQuiz.Models;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/countries")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet("round")]
    public async Task<ActionResult<CountryRoundResponse>> GetRound()
    {
        var round = await _countryService.GenerateRoundAsync();
        return Ok(round);
    }

    [HttpPost("answer")]
    public IActionResult SubmitAnswer([FromQuery] string selected)
    {
        var result = _countryService.EvaluateAnswer(selected);

        return Ok(new
        {
            correct = result.isCorrect,
            winner = result.winner
        });
    }

}
