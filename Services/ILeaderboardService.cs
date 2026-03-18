using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface ILeaderboardService
    {
        LeaderboardEntry AddEntry(string playerName, int score);
        List<LeaderboardEntry> GetEntries();
        List<LeaderboardEntry> GetFiltered(string? name, DateOnly? date);
        LeaderboardEntry Update(string playerName, int newScore);
        void Delete(string playerName);
    }
}

