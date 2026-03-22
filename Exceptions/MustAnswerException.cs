namespace GeographyQuiz.Exceptions
{
    public class MustAnswerException : Exception
    {
        public MustAnswerException(string message) : base(message) { }
    }
}
