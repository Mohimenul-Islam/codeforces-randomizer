namespace CodeforcesRandomizer.Exceptions;

public class GroupLimitExceededException(int limit) 
    : Exception($"Maximum {limit} practice groups allowed per user.");
