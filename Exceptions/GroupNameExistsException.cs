namespace CodeforcesRandomizer.Exceptions;

public class GroupNameExistsException(string name)
    : Exception($"A group named '{name}' already exists.");
