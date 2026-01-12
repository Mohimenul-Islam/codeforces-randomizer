namespace CodeforcesRandomizer.Models;

public class PracticeGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<string> Usernames { get; set; } = [];
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
