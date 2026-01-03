namespace CodeforcesRandomizer.Exceptions;

public class UserNotFoundException(string username)
    : Exception($"User '{username}' not found.")
{
    public string Username { get; } = username;
}
