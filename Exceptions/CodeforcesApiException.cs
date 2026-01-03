namespace CodeforcesRandomizer.Exceptions;

public class CodeforcesApiException : Exception
{
    public int? StatusCode { get; }

    public CodeforcesApiException(string message) : base(message) { }
    public CodeforcesApiException(string message, int statusCode) : base(message) => StatusCode = statusCode;
    public CodeforcesApiException(string message, Exception inner) : base(message, inner) { }
}
