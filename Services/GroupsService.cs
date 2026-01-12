using System.Net;
using CodeforcesRandomizer.Data;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeforcesRandomizer.Services;

public class GroupsService(AppDbContext dbContext, HttpClient httpClient) : IGroupsService
{
    private const int MaxGroupsPerUser = 10;
    private const int MaxUsernamesPerGroup = 20;
    private const string CodeforcesApiUrl = "https://codeforces.com/api/user.info?handles=";

    public async Task<IEnumerable<PracticeGroup>> GetUserGroupsAsync(int userId)
    {
        return await dbContext.PracticeGroups
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<PracticeGroup?> GetByIdAsync(int groupId, int userId)
    {
        return await dbContext.PracticeGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);
    }

    public async Task<PracticeGroup> CreateAsync(int userId, string name, List<string> usernames)
    {
        var groupCount = await dbContext.PracticeGroups.CountAsync(g => g.UserId == userId);
        if (groupCount >= MaxGroupsPerUser)
            throw new GroupLimitExceededException(MaxGroupsPerUser);

        var exists = await dbContext.PracticeGroups.AnyAsync(g => g.UserId == userId && g.Name == name);
        if (exists)
            throw new GroupNameExistsException(name);

        var cleanUsernames = usernames.Distinct().Take(MaxUsernamesPerGroup).ToList();
        await ValidateUsernamesAsync(cleanUsernames);

        var group = new PracticeGroup
        {
            Name = name.Trim(),
            Usernames = cleanUsernames,
            UserId = userId
        };

        dbContext.PracticeGroups.Add(group);
        await dbContext.SaveChangesAsync();

        return group;
    }

    public async Task<PracticeGroup> UpdateAsync(int groupId, int userId, string name, List<string> usernames)
    {
        var group = await dbContext.PracticeGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);

        if (group is null)
            throw new GroupNotFoundException(groupId);

        var nameExists = await dbContext.PracticeGroups
            .AnyAsync(g => g.UserId == userId && g.Name == name && g.Id != groupId);
        if (nameExists)
            throw new GroupNameExistsException(name);

        var cleanUsernames = usernames.Distinct().Take(MaxUsernamesPerGroup).ToList();
        await ValidateUsernamesAsync(cleanUsernames);

        group.Name = name.Trim();
        group.Usernames = cleanUsernames;

        await dbContext.SaveChangesAsync();
        return group;
    }

    public async Task DeleteAsync(int groupId, int userId)
    {
        var group = await dbContext.PracticeGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);

        if (group is null)
            throw new GroupNotFoundException(groupId);

        dbContext.PracticeGroups.Remove(group);
        await dbContext.SaveChangesAsync();
    }

    private async Task ValidateUsernamesAsync(List<string> usernames)
    {
        if (usernames.Count == 0)
            return;

        var handles = string.Join(";", usernames);
        
        try
        {
            var response = await httpClient.GetAsync($"{CodeforcesApiUrl}{handles}");
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var invalidUsers = new List<string>();
                foreach (var username in usernames)
                {
                    var checkResponse = await httpClient.GetAsync($"{CodeforcesApiUrl}{username}");
                    if (checkResponse.StatusCode == HttpStatusCode.BadRequest)
                        invalidUsers.Add(username);
                }
                
                if (invalidUsers.Count > 0)
                    throw new UserNotFoundException(invalidUsers);
            }

            if (!response.IsSuccessStatusCode)
                throw new CodeforcesApiException("Unable to verify Codeforces handles.", (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            throw new CodeforcesApiException("Unable to connect to Codeforces to verify handles.");
        }
    }
}
