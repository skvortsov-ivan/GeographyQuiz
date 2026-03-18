using GeographyQuiz.Exceptions;
using GeographyQuiz.Models;

namespace GeographyQuiz.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private static readonly List<LeaderboardEntry> _entries = new();

        public LeaderboardEntry AddEntry(string playerName, int score)
        {
            var entry = new LeaderboardEntry
            {
                PlayerName = playerName,
                Score = score,
                Date = DateOnly.FromDateTime(DateTime.Today)
            };

            _entries.Add(entry);
            return entry;
        }

        public List<LeaderboardEntry> GetEntries()
        {
            return _entries
                .OrderByDescending(e => e.Score)
                .ToList();
        }

        public List<LeaderboardEntry> GetFiltered(string? name, DateOnly? date)
        {
            var query = _entries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e =>
                    e.PlayerName.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (date.HasValue)
            {
                query = query.Where(e => e.Date == date.Value);
            }

            return query
                .OrderByDescending(e => e.Score)
                .ToList();
        }

        public LeaderboardEntry Update(string playerName, int newScore)
        {
            var entry = _entries.FirstOrDefault(e => e.PlayerName == playerName);
            if (entry == null)
                throw new NotFoundException($"Player '{playerName}' not found.");

            entry.Score = newScore;
            entry.Date = DateOnly.FromDateTime(DateTime.Today);
            return entry;
        }

        public void Delete(string playerName)
        {
            var entry = _entries.FirstOrDefault(e => e.PlayerName == playerName);
            if (entry == null)
                throw new NotFoundException($"Player '{playerName}' not found.");

            _entries.Remove(entry);
        }


    }
}

