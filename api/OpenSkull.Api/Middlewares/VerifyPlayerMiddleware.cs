using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Middleware;

public delegate string VerificationSaltGenerator();

// Why are we doing this like this instead of using an auth library?
// Because OpenSkull isn't *really* about being secure
public class VerifyPlayerMiddleware
{
    public static string PlayerInfoKey = "ValidatedPlayerId";
    public static VerificationSaltGenerator DefaultSaltGenerator = Guid.NewGuid().ToString;

    private readonly RequestDelegate _next;
    private readonly IPlayerStorage _playerStorage;
    private readonly VerificationSaltGenerator _saltGenerator;

    public VerifyPlayerMiddleware(
      RequestDelegate next, 
      IPlayerStorage playerStorage,
      VerificationSaltGenerator? saltGenerator = null
      )
    {
        _next = next;
        _playerStorage = playerStorage;
        if (saltGenerator != null) {
          _saltGenerator = saltGenerator;
        } else {
          _saltGenerator = DefaultSaltGenerator;
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        StringValues rawPlayerId;
        Guid playerId;
        StringValues rawPlayerSecret;
        var gotUserId = context.Request.Headers.TryGetValue("X-OpenSkull-UserId", out rawPlayerId);
        var gotSecret = context.Request.Headers.TryGetValue("X-OpenSkull-UserSecret", out rawPlayerSecret);
        if (gotUserId && gotSecret && Guid.TryParse(rawPlayerId.ToString(), out playerId)) {
          // Validate it
          var parsedPlayerId = await ValidatePlayerId(_playerStorage, _saltGenerator, playerId, rawPlayerSecret);
          if (parsedPlayerId != null) {
            context.Items[PlayerInfoKey] = parsedPlayerId;
          }
        }
        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }

    public static async Task<Guid?> ValidatePlayerId(IPlayerStorage _playerStorage, VerificationSaltGenerator _saltGenerator, Guid playerId, string secret) {
      var player = await _playerStorage.GetPlayerById(playerId);
      if (player.IsSuccess)
      {
        if (player.Value.HashedSecret == HashSecret(secret, player.Value.Salt))
        {
          // Is the real player - set the context
          return playerId;
        }
      }
      if (player.IsFailure && player.Error == StorageError.NotFound)
      {
        string salt = _saltGenerator();
        // New Player - save them
        var createResult = await _playerStorage.CreatePlayer(new Player {
          Id = playerId,
          HashedSecret = HashSecret(secret, salt),
          Salt = salt
        });
        if (createResult.IsSuccess) {
          return playerId;
        }
      }
      return null;
    }

    public static string HashSecret(string secret, string salt) 
    {
      using (var sha = SHA256.Create()) {

        return Encoding.ASCII.GetString(
          sha.ComputeHash(Encoding.ASCII.GetBytes(secret + salt))
        );
      }
    }

    public static Guid? GetValidatedPlayerIdFromContext(HttpContext context)
    {
      if (context.Items.ContainsKey(PlayerInfoKey)) {
        return context.Items[PlayerInfoKey] != null ? (Guid)context.Items[PlayerInfoKey]! : null;
      }
      return null;
    }
}


public static class VerifyPlayerMiddlewareExtensions
{
    public static IApplicationBuilder UseVerifyPlayer(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<VerifyPlayerMiddleware>();
    }
}
