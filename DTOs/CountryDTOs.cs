using System.Text.Json;
using System.Text.Json.Serialization;
using GeographyQuiz.Models;

namespace GeographyQuiz.DTOs
{
    // Response från API Ninjas. Måste vara lowcase för att det är så informationen är lagrad i Json
    public record CountryApiResponse(
        string name,
        JsonElement population
    );

    // DTO som innehåller endast namn på landet för en spelrunda.
    public record CountryRound(string Name);


    // En spelrunda
    public record CountryRoundResponse(
        CountryRound CountryA,
        CountryRound CountryB
    );

    // DTO som innehåller namnet på vinnaren
    public record WinnerDto(string Name);
}

