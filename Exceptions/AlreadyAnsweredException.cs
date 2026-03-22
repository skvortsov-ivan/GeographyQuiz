namespace GeographyQuiz.Exceptions
{
    public class AlreadyAnsweredException : Exception
    {
        public AlreadyAnsweredException(string message) : base(message)
        {
        }
    }
}
