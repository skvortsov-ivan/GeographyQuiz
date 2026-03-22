namespace GeographyQuiz.Models
{
    // Database model representation for the LeaderboardEntry entity
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
        public DateOnly Date { get; set; }
    }
}
