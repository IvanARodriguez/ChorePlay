using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Domain;
using ChorePlay.Api.Shared.Domain.Exceptions;
using ChorePlay.Api.Shared.Mappings;
using Microsoft.AspNetCore.Identity;

namespace ChorePlay.Api.Infrastructure.Repository;

public class UserRepository(UserManager<AppUser> userManager) : IUserRepository
{

    private readonly UserManager<AppUser> _userManager = userManager;
    public async Task<User> UpsertAsync(User user, CancellationToken ct)
    {
        var existingUser = await _userManager.FindByEmailAsync(user.Email);

        if (existingUser is not null)
        {
            // Update basic info (you can extend this as needed)
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.AvatarUrl = user.AvatarUrl;

            var updateResult = await _userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
            {
                throw new ExternalLoginProviderException(
                    "Google",
                    $"Unable to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"
                );
            }

            return existingUser.ToUserDomain();
        }

        // If new user — create
        var newUser = new AppUser
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.Email,
            AvatarUrl = user.AvatarUrl,
            FirstName = user.FirstName,
            EmailConfirmed = true,
            LastName = user.LastName
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            throw new ExternalLoginProviderException(
                "Google",
                $"Unable to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}"
            );
        }

        return newUser.ToUserDomain();
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var foundUser = await _userManager.FindByEmailAsync(email);
        return foundUser?.ToUserDomain();
    }

    public async Task SaveRefreshTokenAsync(Ulid userId, string refreshToken, CancellationToken ct)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException("User not found");
        appUser.RefreshToken = refreshToken;
        await _userManager.UpdateAsync(appUser);
    }
}
