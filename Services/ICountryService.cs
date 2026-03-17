using GeographyQuiz.DTOs;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface ICountryService
    {
        Task<CountryRoundResponse> GenerateRoundAsync(string? previousWinner = null);
        //Task<List<Country>> GetAllCountriesAsync();
        Task<CountryApiResponse> FetchCountryByNameAsync(string name);
        (bool isCorrect, CountryRound winner) EvaluateAnswer(string selected);
        //Task<List<CountryApiResponse>> FetchAllCountriesFromApiAsync();
    }
}

