using GeographyQuiz.DTOs;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface ICountryService
    {
        Task<Country> GetRandomCountryAsync();
        Task<Country> GetCountryByNameAsync(string name);
    }

}

