using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Domain;
using ChorePlay.Api.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ChorePlay.Api.Infrastructure.Repository;

public class UserRepository(UserManager<AppUser> userManager) : IUserRepository
{
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<User> UpsertAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(user.Email);

        if (existingUser is not null)
        {
            // Only update fields we trust from OAuth provider — prevent attacker data injection
            existingUser.FirstName = user.FirstName ?? existingUser.FirstName;
            existingUser.LastName = user.LastName ?? existingUser.LastName;
            existingUser.AvatarUrl = user.AvatarUrl ?? existingUser.AvatarUrl;

            var updateResult = await _userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
                throw new BadRequestException(
                    FormatIdentityErrors("Unable to update OAuth user", updateResult)
                );

            return (await _userManager.FindByEmailAsync(user.Email))!.ToUserDomain();
        }

        var newUser = await CreateOauthNewUserAsync(user, ct);

        return newUser.ToUserDomain();
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(user.Email);

        if (existingUser is not null)
        {
            await HandleExistingUserRegistrationAsync(existingUser, user);
            return existingUser.ToUserDomain();
        }

        var newUser = await CreateNewUserAsync(user, ct);
        return newUser.ToUserDomain();
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var foundUser = await _userManager.FindByEmailAsync(email);
        return foundUser?.ToUserDomain();
    }

    public async Task SaveRefreshTokenAsync(
        Ulid userId,
        string refreshToken,
        DateTime expiration,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var appUser =
            await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException($"User with ID '{userId}' was not found.");

        appUser.RefreshToken = refreshToken;
        appUser.RefreshTokenExpirationDate = expiration;

        var result = await _userManager.UpdateAsync(appUser);

        if (!result.Succeeded)
        {
            throw new BusinessRuleViolationException(
                $"Unable to save refresh token: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
    }

    /// <remarks>
    /// This method requires that the token is hashed before validation.
    /// Use <see cref="IJwtService.HashToken(string)"/> to obtain the correct hashed value.
    /// </remarks>
    public async Task<bool> ValidateRefreshTokenAsync(
        Ulid userId,
        string hashedToken,
        CancellationToken ct
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.RefreshToken == null)
            return false;
        return user.RefreshToken == hashedToken
            && user.RefreshTokenExpirationDate.HasValue
            && user.RefreshTokenExpirationDate.Value > DateTime.UtcNow;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var existingUser =
            await _userManager.FindByIdAsync(user.Id.ToString())
            ?? throw new NotFoundException("User not found");

        await UpdateExistingUserAsync(existingUser, user, ct);
        return existingUser.ToUserDomain();
    }

    // ---------- Private Helpers ----------

    private async Task UpdateExistingUserAsync(
        AppUser existingUser,
        User user,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        UpdateBasicFields(existingUser, user);

        if (!string.IsNullOrWhiteSpace(user.PlainPassword) && existingUser.PasswordHash is null)
        {
            var result = await _userManager.AddPasswordAsync(existingUser, user.PlainPassword);

            if (!result.Succeeded)
                throw new BadRequestException(
                    FormatIdentityErrors("Failed to set password", result)
                );
        }

        var updateResult = await _userManager.UpdateAsync(existingUser);

        if (!updateResult.Succeeded)
            throw new BusinessRuleViolationException(
                FormatIdentityErrors("Unable to update user", updateResult)
            );
    }

    private static void UpdateBasicFields(AppUser target, User source)
    {
        target.FirstName = source.FirstName ?? target.FirstName;
        target.LastName = source.LastName ?? target.LastName;
        target.AvatarUrl = source.AvatarUrl ?? target.AvatarUrl;
    }

    private async Task<AppUser> CreateOauthNewUserAsync(User user, CancellationToken ct)
    {
        var newUser = user.ToAppUserDomain();

        var result = await _userManager.CreateAsync(newUser);

        if (!result.Succeeded)
            throw new BadRequestException(
                FormatIdentityErrors("Unable to create OAuth user", result)
            );

        return newUser;
    }

    private async Task<AppUser> CreateNewUserAsync(User user, CancellationToken ct)
    {
        var newUser = user.ToAppUserDomain();
        newUser.EmailConfirmed = false;

        IdentityResult result = !string.IsNullOrWhiteSpace(user.PlainPassword)
            ? await _userManager.CreateAsync(newUser, user.PlainPassword)
            : await _userManager.CreateAsync(newUser);

        if (!result.Succeeded)
            throw new BadRequestException(FormatIdentityErrors("Unable to create user", result));

        return newUser;
    }

    private async Task HandleExistingUserRegistrationAsync(AppUser existingUser, User user)
    {
        if (!string.IsNullOrWhiteSpace(existingUser.PasswordHash))
            throw new ConflictException("Account creation failed due to a conflict");

        if (string.IsNullOrWhiteSpace(user.PlainPassword))
            throw new BadRequestException("A password is required to register this user.");

        var result = await _userManager.AddPasswordAsync(existingUser, user.PlainPassword);
        if (!result.Succeeded)
            throw new BadRequestException(FormatIdentityErrors("Failed to set password", result));
    }

    private static string FormatIdentityErrors(string message, IdentityResult result) =>
        $"{message}: {string.Join(", ", result.Errors.Select(e => e.Description))}";

    public async Task<bool> PasswordIsValid(User user, string password, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var appUser = await _userManager.FindByEmailAsync(user.Email);
        if (appUser is null)
            return false;

        return await _userManager.CheckPasswordAsync(appUser, password);
    }
}
