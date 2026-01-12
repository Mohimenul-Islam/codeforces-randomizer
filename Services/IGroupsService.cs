using CodeforcesRandomizer.Models;

namespace CodeforcesRandomizer.Services;

public interface IGroupsService
{
    Task<IEnumerable<PracticeGroup>> GetUserGroupsAsync(int userId);
    Task<PracticeGroup?> GetByIdAsync(int groupId, int userId);
    Task<PracticeGroup> CreateAsync(int userId, string name, List<string> usernames);
    Task<PracticeGroup> UpdateAsync(int groupId, int userId, string name, List<string> usernames);
    Task DeleteAsync(int groupId, int userId);
}
