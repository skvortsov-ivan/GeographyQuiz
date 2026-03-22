using GeographyQuiz.DTOs;
using GeographyQuiz.Exceptions;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private static readonly List<LeaderboardEntry> _entries = new();

        // Adds a new leaderboard entry
        public Task<LeaderboardEntry> AddEntryAsync(string playerName, int score)
        {
            var entry = new LeaderboardEntry
            {
                PlayerName = playerName,
                Score = score,
                Date = DateOnly.FromDateTime(DateTime.Today)
            };
            Console.WriteLine($"Entries count: {_entries.Count}");
            _entries.Add(entry);
            return Task.FromResult(entry);
        }

        // Returns all entries sorted by score
        public Task<List<LeaderboardEntry>> GetEntriesAsync()
        {
            var result = _entries
                .OrderByDescending(e => e.Score)
                .ToList();

            return Task.FromResult(result);
        }

        // Returns filtered leaderboard results (by name and/or date)
        public Task<List<LeaderboardEntry>> GetFilteredAsync(string? name, DateOnly? date)
        {
            var query = _entries.AsQueryable();

            // Filter by player name
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e =>
                    e.PlayerName.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by date
            if (date.HasValue)
            {
                query = query.Where(e => e.Date == date.Value);
            }

            var result = query
                .OrderByDescending(e => e.Score)
                .ToList();

            return Task.FromResult(result);
        }

        // Updates an existing player's score
        public Task<LeaderboardEntry> UpdateAsync(string playerName, int newScore)
        {
            var entry = _entries.FirstOrDefault(e => e.PlayerName == playerName);
            if (entry == null)
                throw new NotFoundException($"Player '{playerName}' not found.");

            entry.Score = newScore;
            entry.Date = DateOnly.FromDateTime(DateTime.Today);
            
            return Task.FromResult(entry);
        }

        // Deletes a leaderboard entry by player name
        public Task DeleteAsync(string playerName)
        {
            var entry = _entries.FirstOrDefault(e => e.PlayerName == playerName);
            if (entry == null)
                throw new NotFoundException($"Player '{playerName}' not found.");

            _entries.Remove(entry);
            return Task.CompletedTask;
        }

        // Returns a paginated leaderboard result
        public Task<PaginatedLeaderboardResponse<LeaderboardEntry>> GetPagedAsync(int page, int pageSize)
        {
            // Max page size of 10
            if (pageSize > 10)
                pageSize = 10;

            if (page < 1)
                page = 1;

            var sorted = _entries
                .OrderByDescending(e => e.Score)
                .ToList();

            int totalCount = sorted.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginatedLeaderboardResponse<LeaderboardEntry>(
                Items: items,
                Page: page,
                PageSize: pageSize,
                TotalCount: totalCount,
                TotalPages: totalPages
            );
            return Task.FromResult(response);
        }
    }
}

