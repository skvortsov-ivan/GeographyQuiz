using GeographyQuiz.Data;
using GeographyQuiz.DTOs;
using GeographyQuiz.Exceptions;
using GeographyQuiz.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace GeographyQuiz.Services
{
    public class CountryService : ICountryService
    {
        private readonly HttpClient _httpClient;
        private readonly HybridCache _cache;


        public CountryService(HttpClient httpClient, HybridCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<CountryRoundResponse> GenerateRoundAsync()
        {
            Console.WriteLine(">>> ROUND START");

            // 1. Pick two random UN countries
            var random = new Random();
            var nameA = UNCountries.All[random.Next(UNCountries.All.Count)];
            var nameB = UNCountries.All[random.Next(UNCountries.All.Count)];

            while (nameA == nameB)
                nameB = UNCountries.All[random.Next(UNCountries.All.Count)];

            Console.WriteLine($">>> PICKED: {nameA} vs {nameB}");

            // 2. Fetch each country
            var countryA = await FetchCountryByNameAsync(nameA);
            var countryB = await FetchCountryByNameAsync(nameB);

            long popA = ConvertPopulation(countryA.population);
            long popB = ConvertPopulation(countryB.population);

            string correct = popA > popB ? "A" : "B";

            Console.WriteLine(">>> ROUND END");

            return new CountryRoundResponse(
                new CountryRound(countryA.name, popA),
                new CountryRound(countryB.name, popB),
                correct
            );
        }

        public async Task<CountryApiResponse> FetchCountryByNameAsync(string name)
        {
            // Each country has it's own cache key
            var cacheKey = $"country_{name.ToLower()}";

            // Check if country exists in cash. If not then go into the lambda and make API call
            var country = await _cache.GetOrCreateAsync(cacheKey,
                async cancel =>
                {
                    Console.WriteLine($">>> CACHE MISS: Fetching {name} from API");

                    // API call
                    var response = await _httpClient.GetAsync($"?name={name}", cancel);
                    response.EnsureSuccessStatusCode();


                    var json = await response.Content.ReadAsStringAsync(cancel);
                    Console.WriteLine("RAW JSON:");
                    Console.WriteLine(json);

                    var list = JsonSerializer.Deserialize<List<CountryApiResponse>>(json);
                    Console.WriteLine("DESERIALIZED OBJECT:");
                    Console.WriteLine($"Count: {list?.Count}");

                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            Console.WriteLine($"Name: {item.name}");
                            Console.WriteLine($"Population (JsonElement): {item.population}");
                            Console.WriteLine($"Population ValueKind: {item.population.ValueKind}");
                            Console.WriteLine("-----");
                        }
                    }


                    return list?.FirstOrDefault();
                }
            );

            if (country == null)
                throw new NotFoundException($"Country '{name}' not found in API");

            return country;
        }


        private long ConvertPopulation(JsonElement value)
        {
            if (value is JsonElement j)
            {
                //Console.WriteLine(j.ValueKind);
                //Console.WriteLine(j.ToString());

                if (j.ValueKind == JsonValueKind.Number)
                    return j.TryGetDouble(out double d) ? (long)d : 0;

                if (j.ValueKind == JsonValueKind.String)
                    return long.TryParse(j.GetString(), out long s) ? s : 0;

                return 0;
            }

            return 0;
        }
    }
}


