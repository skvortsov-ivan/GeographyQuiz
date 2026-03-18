using GeographyQuiz.DTOs;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface IGameService
    {
        Task<CountryRoundResponse> GenerateRoundAsync();
        (bool isCorrect, Country winner) EvaluateAnswer(string selected);
        void ResetGame();
    }

}
