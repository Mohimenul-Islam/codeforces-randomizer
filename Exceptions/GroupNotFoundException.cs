namespace CodeforcesRandomizer.Exceptions;

public class GroupNotFoundException(int groupId)
    : Exception($"Practice group with ID {groupId} not found.");
