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
    public async Task<User> CreateWithGoogleAsync(User user, CancellationToken ct)
    {
        AppUser appUser = new()
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.Email,
            AvatarUrl = user.AvatarUrl,
            FirstName = user.FirstName,
            EmailConfirmed = true,
            LastName = user.LastName
        };
        var result = await _userManager.CreateAsync(appUser);

        if (!result.Succeeded)
            throw new ExternalLoginProviderException("Google",
                $"Unable to create user: {string.Join(", ",
                    result.Errors.Select(x => x.Description))}");

        return appUser.ToUserDomain();
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
