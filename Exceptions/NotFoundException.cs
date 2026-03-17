namespace GeographyQuiz.Exceptions
{
    /// <summary>
    /// Kastas när en resurs inte hittas.
    /// Mappas till HTTP 404 i Custom Exception Middleware.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
