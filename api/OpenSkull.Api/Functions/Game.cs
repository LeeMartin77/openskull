using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Functions;

public enum GameCreationError {
  InvalidNumberOfPlayers,
  DuplicatePlayer
}

public enum GameTurnError {
  InvalidNumberOfPlayers,
  DuplicatePlayer
}

public static class GameFunctions {
  private const int MAX_PLAYERS = 6;
  private const int MIN_PLAYERS = 3;
  private const int CARD_FLOWER_COUNT = 3;
  private const int CARD_SKULL_COUNT = 1;
  public static Result<Game, GameCreationError> CreateNew(Guid[] playerIds) {
    if (playerIds == null || playerIds.Length > MAX_PLAYERS || playerIds.Length < MIN_PLAYERS) {
      return Result.Failure<Game, GameCreationError>(GameCreationError.InvalidNumberOfPlayers);
    }
    if (playerIds.Length != playerIds.Distinct().Count()) {
      return Result.Failure<Game, GameCreationError>(GameCreationError.DuplicatePlayer);
    }
    Card[][] playerCards = new Card[playerIds.Length][];
    Card[][] playerTurn = new Card[playerIds.Length][];
    for (int i = 0; i < playerCards.Length; i++) {
      playerCards[i] = new Card[CARD_FLOWER_COUNT + CARD_SKULL_COUNT] {
        new Card { Id = Guid.NewGuid(), Type = CardType.Flower, State = CardState.Hidden },
        new Card { Id = Guid.NewGuid(), Type = CardType.Flower, State = CardState.Hidden },
        new Card { Id = Guid.NewGuid(), Type = CardType.Flower, State = CardState.Hidden },
        new Card { Id = Guid.NewGuid(), Type = CardType.Skull, State = CardState.Hidden },
      };

    }
    return Result.Success<Game, GameCreationError>(new Game {
      Id = new Guid(),
      PlayerIds = playerIds,
      PlayerCards = playerCards,
      PlayerPoints = new int[playerIds.Length],
      RoundPlayerCards = new List<List<Card>[]>() { new List<Card>[playerIds.Length] }
    });
  }

  // public static Result<Game, GameTurnError> TurnPlayCard(Game game, Guid player, Guid cardId) {}
  // public static Result<Game, GameTurnError> TurnPlaceBid(Game game, Guid player, int bid) {}
  // public static Result<(Game, boolean), GameTurnError> TurnFlipPlayerCard(Game game, Guid player, Guid targetPlayer) {}
  // public static Game ResolveRound(Game game) {}
}