using GeographyQuiz.Data;
using GeographyQuiz.DTOs;
using GeographyQuiz.Exceptions;
using GeographyQuiz.Models;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace GeographyQuiz.Services
{
    public class CountryService : ICountryService
    {
        private readonly HttpClient _httpClient;
        private readonly HybridCache _cache;
        private readonly Random _random = new();

        public CountryService(HttpClient httpClient, HybridCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        // ------------------------------------------------------------
        // Fetch a country by name (API + cache)
        // ------------------------------------------------------------
        public async Task<Country> GetCountryByNameAsync(string name)
        {
            var cacheKey = $"country_{name.ToLower()}";

            var countryData = await _cache.GetOrCreateAsync(cacheKey,
                async cancel =>
                {
                    var response = await _httpClient.GetAsync($"?name={name}", cancel);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(cancel);
                    var popAndNameData = JsonSerializer.Deserialize<List<CountryApiResponse>>(json);

                    return popAndNameData?.FirstOrDefault();
                });

            if (countryData == null)
                throw new NotFoundException($"Country '{name}' not found in API");

            return ConvertToCountry(countryData);
        }

        // ------------------------------------------------------------
        // Get a random UN country
        // ------------------------------------------------------------
        public async Task<Country> GetRandomCountryAsync()
        {
            var name = UNCountries.All[_random.Next(UNCountries.All.Count)];
            return await GetCountryByNameAsync(name);
        }
      
        private Country ConvertToCountry(CountryApiResponse countryData)
        {
            return new Country
            {
                Name = countryData.name,
                Population = ConvertPopulation(countryData.population)
            };
        }

        private long ConvertPopulation(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Number &&
                value.TryGetDouble(out double d))
                return (long)d;

            if (value.ValueKind == JsonValueKind.String &&
                long.TryParse(value.GetString(), out long s))
                return s;

            return 0;
        }
    }
}
