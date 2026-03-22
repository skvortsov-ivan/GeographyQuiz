namespace GeographyQuiz.DTOs
{
    public record PaginatedLeaderboardResponse<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );
}
