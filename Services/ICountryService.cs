using GeographyQuiz.DTOs;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface ICountryService
    {
        Task<CountryRoundResponse> GenerateRoundAsync();
        //Task<List<Country>> GetAllCountriesAsync();
        Task<CountryApiResponse> FetchCountryByNameAsync(string name);
        //Task<List<CountryApiResponse>> FetchAllCountriesFromApiAsync();

    }
}

