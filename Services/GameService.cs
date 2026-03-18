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

        if (!_alreadyAnswered)
            throw new MustAnswerException("You must answer first before proceeding to the next round.");

        _alreadyAnswered = false;

        _roundCount++;
        _currentRound = _roundCount;

        if (_roundCount == MaxRounds)
            _isGameOver = true;

        // A = previous winner or random
        if (_previousWinner == null)
        {
            _countryA = await _countryService.GetRandomCountryAsync();
        }
        else
        {
            _countryA = _previousWinner;
        }

        // B = new challenger
        _countryB = await _countryService.GetRandomCountryAsync();

        while (_countryB.Name == _countryA.Name)
            _countryB = await _countryService.GetRandomCountryAsync();

        return new CountryRoundResponse(_countryA, _countryB);
    }

    public (bool isCorrect, Country winner) EvaluateAnswer(string selected)
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

        return (isCorrect, _previousWinner);
    }

    public void ResetGame()
    {
        _roundCount = 0;
        _currentRound = 0;

        _isGameOver = false;

        _previousWinner = null;
        _countryA = null;
        _countryB = null;
        _alreadyAnswered = false;
    }
}
