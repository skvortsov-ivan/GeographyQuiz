using System.Text.Json;
using System.Text.Json.Serialization;
using GeographyQuiz.Models;

namespace GeographyQuiz.DTOs
{
    // 1. Response från API Ninjas. Måste vara lowcase för att det är så informationen är lagrad i Json
    public record CountryApiResponse(
        string name,
        JsonElement population
    );

    // 2. Det klienten får i en spelrunda
    public record CountryRound(
        string Name,
        long Population
    );

    // 3. En spelrunda
    public record CountryRoundResponse(
        Country CountryA,
        Country CountryB
    );
}

