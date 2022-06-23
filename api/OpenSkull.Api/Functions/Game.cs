using System.Linq;
using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Functions;

public enum GameCreationError {
  InvalidNumberOfPlayers,
  DuplicatePlayer
}

public enum GameTurnError {
  InvalidPlayerId,
  InvalidCardId,
  CannotBidYet,
  CannotPlayCardAfterBid,
  MaxBidExceeded,
  MinBidNotMet,
  BiddingHasFinished
}

public static class GameFunctions {
  private const int MAX_PLAYERS = 6;
  private const int MIN_PLAYERS = 3;
  private const int CARD_FLOWER_COUNT = 3;
  private const int CARD_SKULL_COUNT = 1;
  private const int SKIP_BIDDING_VALUE = -1;

  public static Result<Game, GameCreationError> CreateNew(Guid[] playerIds) {
    if (playerIds == null || playerIds.Length > MAX_PLAYERS || playerIds.Length < MIN_PLAYERS) {
      return GameCreationError.InvalidNumberOfPlayers;
    }
    if (playerIds.Length != playerIds.Distinct().Count()) {
      return GameCreationError.DuplicatePlayer;
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

    var round = new List<Guid>[playerIds.Length];

    for (int i = 0; i < round.Length; i++) {
      round[i] = new List<Guid>();
    }

    return new Game {
      Id = Guid.NewGuid(),
      PlayerIds = playerIds,
      PlayerCards = playerCards,
      PlayerPoints = new int[playerIds.Length],
      RoundPlayerCardIds = new List<List<Guid>[]>() { round },
      RoundBids = new List<int[]>() { new int[playerIds.Length] },
    };
  }

  public static Result<Game, GameTurnError> TurnPlayCard(Game game, Guid playerId, Guid cardId) {
    int playerIndex = Array.IndexOf(game.PlayerIds, playerId);
    if (playerIndex == -1 || playerIndex != game.ActivePlayerIndex) {
      return GameTurnError.InvalidPlayerId;
    }
    if (game.RoundBids.Last().Any(x => x > 0)) {
      return GameTurnError.CannotPlayCardAfterBid;
    }
    if (!game.PlayerCards[playerIndex].Select(x => x.Id).Contains(cardId)
      || game.PlayerCards[playerIndex].First(x => x.Id == cardId).State != CardState.Hidden
      || game.RoundPlayerCardIds.Last()[playerIndex].Contains(cardId)) {
      return GameTurnError.InvalidCardId;
    }
    game.RoundPlayerCardIds.Last()[playerIndex].Add(cardId);
    game.ActivePlayerIndex += 1;
    if (game.ActivePlayerIndex > game.PlayerIds.Length - 1) {
      game.ActivePlayerIndex = 0;
    }
    return game;
  }
  
  public static Result<Game, GameTurnError> TurnPlaceBid(Game game, Guid playerId, int bid) {
    int playerIndex = Array.IndexOf(game.PlayerIds, playerId);
    if (playerIndex == -1 || playerIndex != game.ActivePlayerIndex) {
      return GameTurnError.InvalidPlayerId;
    }
    if (!game.RoundPlayerCardIds.Last().All(x => x.Count() > 0)) {
      return GameTurnError.CannotBidYet;
    }
    if (bid > game.RoundPlayerCardIds.Last().Sum(x => x.Count())) {
      return GameTurnError.MaxBidExceeded;
    }
    if (bid > SKIP_BIDDING_VALUE && bid <= game.RoundBids.Last().Max()) {
      return GameTurnError.MinBidNotMet;
    }
    if (game.RoundBids.Last().Count(x => x != SKIP_BIDDING_VALUE) == 1) {
      return GameTurnError.BiddingHasFinished;
    }
    game.RoundBids.Last()[playerIndex] = bid;
    game.ActivePlayerIndex += 1;
    if (game.ActivePlayerIndex > game.PlayerIds.Length - 1) {
      game.ActivePlayerIndex = 0;
    }
    while (game.RoundBids.Last()[game.ActivePlayerIndex] == -1) {
      game.ActivePlayerIndex += 1;
      if (game.ActivePlayerIndex > game.PlayerIds.Length - 1) {
        game.ActivePlayerIndex = 0;
      }
    }
    return game;
  }
  // public static Result<(Game, boolean), GameTurnError> TurnFlipPlayerCard(Game game, Guid player, Guid targetPlayer) {}
  // public static Game ResolveRound(Game game) {}
}