using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GeographyQuiz.DTOs
{
    public record LeaderboardRequest
    {
        [Required(ErrorMessage = "Player name is required.")]
        public string PlayerName { get; set; } = "";
        [Range(0, 5, ErrorMessage = "Score must be between 0 and 5.")]
        public int Score { get; set; }
    }

    public record UpdateLeaderboardRequest
    {
        [Required(ErrorMessage = "Player name is required.")]
        public string PlayerName { get; set; } = "";

        [Range(0, 5, ErrorMessage = "Score must be between 0 and 5.")]
        public int Score { get; set; }
    }

};
