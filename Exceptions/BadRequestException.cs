namespace GeographyQuiz.Exceptions
{
    /// <summary>
    /// Kastas när klienten skickar ett ogiltigt anrop.
    /// Mappas till HTTP 400 i Custom Exception Middleware.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
