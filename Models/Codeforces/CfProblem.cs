namespace CodeforcesRandomizer.Models.Codeforces;

public record CfProblem(
    int ContestId,
    string Index,
    string Name,
    int? Rating,
    string[] Tags
)
{
    public string ProblemId => $"{ContestId}{Index}";
    public string Url => $"https://codeforces.com/problemset/problem/{ContestId}/{Index}";
}
