namespace GeographyQuiz.DTOs
{
    // Paginated response for leaderboard results
    public record PaginatedLeaderboardResponse<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );
}
