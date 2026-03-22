using System.Text.Json;
using System.Text.Json.Serialization;
using GeographyQuiz.Models;

namespace GeographyQuiz.DTOs
{
    // Response from API Ninjas. Must be lower case because that's how the information is stored in the Json.
    public record CountryApiResponse(
        string name,
        JsonElement population
    );

    // Represents a single country shown in a round
    public record CountryRound(string Name);


    // Response containing the two countries for the current round
    public record CountryRoundResponse(
        CountryRound CountryA,
        CountryRound CountryB
    );

    // Represents the winner of a round
    public record WinnerDto(string Name);

    // Represents a cached country
    public record CachedCountry(string Name, long Population);
}

