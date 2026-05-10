using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

    Task<UserProfile> AddAsync(UserProfile profile, CancellationToken cancellationToken = default);

    Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default);
}
