namespace CodeforcesRandomizer.Models;

public record ProblemDto(
    string ProblemId,
    string Name,
    int? Rating,
    string[] Tags,
    string Url
);
