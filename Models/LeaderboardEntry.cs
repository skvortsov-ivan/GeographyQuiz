namespace GeographyQuiz.Models
{
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
        public DateOnly Date { get; set; }
    }
}
