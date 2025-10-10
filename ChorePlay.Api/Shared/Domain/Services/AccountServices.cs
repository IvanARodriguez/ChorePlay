using ChorePlay.Api.Features.Auth.DTOs;
using ChorePlay.Api.Features.Auth.Login;
using ChorePlay.Api.Features.Auth.Register;
using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Domain.Exceptions;
using ChorePlay.Api.Shared.Jwt;

namespace ChorePlay.Api.Shared.Domain.Services;

public sealed class AccountService(IUserRepository users, IJwtService jwtService)
{
  private readonly IUserRepository _users = users;
  private readonly IJwtService _jwtService = jwtService;

  public async Task<AuthResultDto> LoginOrRegisterWithGoogleAsync(
    string email, string firstName, string lastName, string picture, CancellationToken ct)
  {
    User user = await _users.UpsertAsync(new User(
      Ulid.NewUlid(),
      email,
      firstName,
      picture,
      lastName
    ), ct) ?? throw new ForbiddenException("failed to update user from google data");

    return await GenerateAuthResultAsync(user, ct);
  }

  public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken ct)
  {
    var user = new User(
      Id: Ulid.NewUlid(),
      FirstName: request.FirstName,
      LastName: request.LastName,
      Email: request.Email,
      AvatarUrl: null
    )
    {
      PlainPassword = request.Password
    };

    User newUser = await _users.CreateAsync(user, ct);

    user.PlainPassword = null;

    return newUser;
  }

  public async Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken ct)
  {
    var user = await _users.FindByEmailAsync(request.Email, ct) ?? throw new NotFoundException("Failed to login, please try again");

    if (!await _users.PasswordIsValid(user, request.Password, ct))
    {
      throw new ForbiddenException("Failed to login, check your credentials and try again");
    }

    return await GenerateAuthResultAsync(user, ct);
  }

  private async Task<AuthResultDto> GenerateAuthResultAsync(User user, CancellationToken ct)
  {
    var (accessToken, accessTokenExpiresAtUtc) = _jwtService.GenerateJwtToken(user);
    var (refreshToken, refreshTokenExpiresAtUtc) = _jwtService.GenerateRefreshToken();
    var hashToken = _jwtService.HashToken(refreshToken);

    await _users.SaveRefreshTokenAsync(user.Id, hashToken, refreshTokenExpiresAtUtc, ct);

    return new AuthResultDto(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.AvatarUrl,
        user.EmailConfirmed,
        user.OAuthEmailConfirmed,
        accessToken,
        accessTokenExpiresAtUtc,
        refreshToken,
        refreshTokenExpiresAtUtc
    );
  }
}
