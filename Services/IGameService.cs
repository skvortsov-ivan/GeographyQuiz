using GeographyQuiz.DTOs;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface IGameService
    {
        Task<CountryRoundResponse> GenerateRoundAsync();
        Task<(bool isCorrect, WinnerDto winner)> EvaluateAnswer(string selected);
        Task ResetGame();
    }

}
