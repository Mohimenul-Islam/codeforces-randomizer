namespace CodeforcesRandomizer.Exceptions;

public class UserNotFoundException : Exception
{
    public IReadOnlyList<string> Usernames { get; }

    public UserNotFoundException(string username)
        : base($"User '{username}' not found.")
    {
        Usernames = [username];
    }

    public UserNotFoundException(IEnumerable<string> usernames)
        : base($"Users not found: {string.Join(", ", usernames.Select(u => $"'{u}'"))}")
    {
        Usernames = usernames.ToList();
    }
}
