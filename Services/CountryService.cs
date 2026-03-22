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
        private static int _apiCallAmount = 0;

        public CountryService(HttpClient httpClient, HybridCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        // Fetch a country by name (API + cache)
        public async Task<Country> GetCountryByNameAsync(string name)
        {
            var cacheKey = $"country_{name.ToLower()}";

            bool cacheMiss = false;

            var totalSw = System.Diagnostics.Stopwatch.StartNew();
            // Retrieve from cache or fetch from API if missing
            var cached = await _cache.GetOrCreateAsync(cacheKey,
                async cancel =>
                {
                    cacheMiss = true;

                    _apiCallAmount++;
                    Console.WriteLine($"Making API call number {_apiCallAmount}");

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var response = await _httpClient.GetAsync($"?name={name}", cancel);
                    sw.Stop();

                    Console.WriteLine($"[CACHE MISS] Fetching country: {name} took {sw.ElapsedMilliseconds} ms");

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(cancel);
                    var apiData = JsonSerializer.Deserialize<List<CountryApiResponse>>(json);

                    var data = apiData?.FirstOrDefault();
                    if (data == null)
                        throw new NotFoundException($"Country '{name}' not found in API");

                    // Store a serializable version of Json into the cache
                    return new CachedCountry(
                        data.name,
                        ConvertPopulation(data.population)
                    );
                });

            totalSw.Stop();
            if (!cacheMiss)
            {
                Console.WriteLine($"[CACHE HIT] Returning cached data for {name} in {totalSw.ElapsedMilliseconds} ms");
            }

            // Convert cached model back to your real Country model
            return new Country
            {
                Name = cached.Name,
                Population = cached.Population
            };
        }

        // Get a random UN country
        public async Task<Country> GetRandomCountryAsync()
        {
            var name = UNCountries.All[_random.Next(UNCountries.All.Count)];
            return await GetCountryByNameAsync(name);
        }

        // Converts API response into a Country model
        private Country ConvertToCountry(CountryApiResponse countryData)
        {
            return new Country
            {
                Name = countryData.name,
                Population = ConvertPopulation(countryData.population)
            };
        }

        // Converts JsonElement population value
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
