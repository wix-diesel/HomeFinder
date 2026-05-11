using HomeFinder.Application.Repositories;
using HomeFinder.Core.Entities;
using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Repositories;

public class UserProfileRepository(ItemDbContext dbContext) : IUserProfileRepository
{
    public async Task<UserProfile?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserProfiles
            .FirstOrDefaultAsync(x => x.EntraObjectId == entraObjectId, cancellationToken);
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserProfiles.FindAsync([userId], cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        dbContext.UserProfiles.Add(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        dbContext.UserProfiles.Update(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }

}
