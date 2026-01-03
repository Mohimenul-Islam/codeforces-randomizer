namespace CodeforcesRandomizer.Models.Codeforces;

public record CfSubmission(
    int Id,
    int ContestId,
    CfSubmissionProblem Problem,
    string Verdict
);

public record CfSubmissionProblem(
    int ContestId,
    string Index,
    string Name
)
{
    public string ProblemId => $"{ContestId}{Index}";
}
