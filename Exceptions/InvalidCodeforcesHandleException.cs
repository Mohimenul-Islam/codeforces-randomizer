namespace CodeforcesRandomizer.Exceptions;

public class InvalidCodeforcesHandleException(string handle) 
    : Exception($"Codeforces handle '{handle}' does not exist.");
