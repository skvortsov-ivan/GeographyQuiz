using GeographyQuiz.DTOs;
using GeographyQuiz.Exceptions;
using GeographyQuiz.Models;
using GeographyQuiz.Services;

public class GameService : IGameService
{
    private readonly ICountryService _countryService;

    private const int MaxRounds = 5;

    private static int _roundCount = 0;
    private static bool _alreadyAnswered = false;

    private static Country? _previousWinner;
    private static Country? _countryA;
    private static Country? _countryB;

    private static int _currentRound = 0;
    private static bool _isGameOver = false;

    public GameService(ICountryService countryService)
    {
        _countryService = countryService;
    }

    public async Task<CountryRoundResponse> GenerateRoundAsync()
    {
        if (_isGameOver)
            throw new GameOverException("Game over. You have reached the 5-round limit.");

        if (_roundCount > 0 && !_alreadyAnswered)
            throw new MustAnswerException("You must answer first before proceeding to the next round.");

        Country? nextA;
        Country? nextB;

        if (_previousWinner == null)
        {
            nextA = await _countryService.GetRandomCountryAsync();
        }
        else
        {
            nextA = _previousWinner;
        }

        nextB = await _countryService.GetRandomCountryAsync();

        if (nextA == null || nextB == null)
            throw new BadRequestException("Failed to fetch countries. Try again.");

        int safety = 0;
        while (nextB.Name == nextA.Name && safety < 5)
        {
            nextB = await _countryService.GetRandomCountryAsync();
            safety++;
        }

        if (nextB == null)
            throw new BadRequestException("Failed to fetch a valid challenger country.");

        _alreadyAnswered = false;

        _roundCount++;
        _currentRound = _roundCount;

        if (_roundCount == MaxRounds)
            _isGameOver = true;

        _countryA = nextA;
        _countryB = nextB;

        return new CountryRoundResponse(new CountryRound(_countryA.Name), new CountryRound(_countryB.Name)
 );
    }

    public Task<(bool isCorrect, WinnerDto winner)> EvaluateAnswer(string selected)
    {
        Country chosen;
        Country other;

        if (_countryA == null || _countryB == null)
            throw new BadRequestException("No round has been generated yet.");

        if (_alreadyAnswered)
            throw new AlreadyAnsweredException("You have already answered this round.");

        _alreadyAnswered = true;

        // Determine which country the player selected
        if (selected == "A")
        {
            chosen = _countryA;
            other = _countryB;
        }
        else
        {
            chosen = _countryB;
            other = _countryA;
        }

        bool isCorrect = chosen.Population > other.Population;

        // Determine the winner
        if (isCorrect)
        {
            _previousWinner = chosen;
        }
        else
        {
            _previousWinner = other;
        }

        var winnerDto = new WinnerDto(_previousWinner.Name);
        
        return Task.FromResult((isCorrect, winnerDto));
    }

    public Task ResetGame()
    {
        _roundCount = 0;
        _currentRound = 0;

        _isGameOver = false;

        _previousWinner = null;
        _countryA = null;
        _countryB = null;
        _alreadyAnswered = false;

        return Task.CompletedTask;
    }
}
