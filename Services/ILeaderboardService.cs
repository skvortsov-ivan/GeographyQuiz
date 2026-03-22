using GeographyQuiz.DTOs;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public interface ILeaderboardService
    {
        Task<LeaderboardEntry> AddEntryAsync(string playerName, int score);
        Task<List<LeaderboardEntry>> GetEntriesAsync();
        Task<List<LeaderboardEntry>> GetFilteredAsync(string? name, DateOnly? date);
        Task<PaginatedLeaderboardResponse<LeaderboardEntry>> GetPagedAsync(int page, int pageSize);
        Task<LeaderboardEntry> UpdateAsync(string playerName, int newScore);
        Task DeleteAsync(string playerName);
    }
}

