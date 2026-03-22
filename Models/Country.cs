using System.Text.Json;

namespace GeographyQuiz.Models
{
    // Database model representation for the Country entity
    public class Country
    {
        public string Name { get; set; } = "";
        public long Population { get; set; }
    }
}
