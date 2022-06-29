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
  BiddingHasFinished,
  MustRevealAllOwnCardsFirst,
  NoCardsLeftToFlip,
  GameHasFinished
}


public delegate Result<Game, GameCreationError> GameCreateNew(Guid[] playerIds);

public delegate Result<Game, GameTurnError> TurnPlayCard(Game game, Guid playerId, Guid cardId);

public delegate Result<Game, GameTurnError> TurnPlaceBid(Game game, Guid playerId, int bid);

public delegate Result<Game, GameTurnError> TurnFlipCard(Game game, Guid playerId, int targetPlayerIndex);


public static class GameFunctions {
  public const int MAX_PLAYERS = 6;
  public const int MIN_PLAYERS = 3;
  private const int CARD_FLOWER_COUNT = 3;
  private const int CARD_SKULL_COUNT = 1;
  public const int SKIP_BIDDING_VALUE = -1;
  private const int ROUNDS_TO_WIN = 2;


  public static string[] GameTurnErrorMessage = new string[] {
    "Invalid Player Id",
    "Invalid Card Id",
    "Cannot Bid Yet",
    "Cannot Play Card After Bid",
    "Max Bid Exceeded",
    "Min Bid NotMet",
    "Bidding Has Finished",
    "Must Reveal All Own Cards First",
    "No Cards Left To Flip",
    "Game Has Finished"
  };

  private static List<Guid>[] _GenerateRoundPlayerCardIds(int playerCount) {
    var round = new List<Guid>[playerCount];

    for (int i = 0; i < round.Length; i++) {
      round[i] = new List<Guid>();
    }

    return round;
  }

  private static Game _IncrementActivePlayer(Game game) {
      game.ActivePlayerIndex += 1;
      if (game.ActivePlayerIndex > game.PlayerIds.Length - 1) {
        game.ActivePlayerIndex = 0;
      }
      return game;
  }

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
        new Card { Id = Guid.NewGuid(), Type = CardType.Flower, State = CardState.InPlay },
        new Card { Id = Guid.NewGuid(), Type = CardType.Flower, State = CardState.InPlay },
        new Card { Id = Guid.NewGuid(), Type = CardType.Flower, State = CardState.InPlay },
        new Card { Id = Guid.NewGuid(), Type = CardType.Skull, State = CardState.InPlay },
      };
    }

    return new Game {
      PlayerIds = playerIds,
      PlayerCards = playerCards,
      RoundPlayerCardIds = new List<List<Guid>[]>() { _GenerateRoundPlayerCardIds(playerIds.Length) },
      RoundBids = new List<int[]>() { new int[playerIds.Length] },
      RoundRevealedCardPlayerIndexes = new List<List<int>>() { new List<int>() },
      RoundWinPlayerIndexes = new List<int>()
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
      || game.PlayerCards[playerIndex].First(x => x.Id == cardId).State != CardState.InPlay
      || game.RoundPlayerCardIds.Last()[playerIndex].Contains(cardId)) {
      return GameTurnError.InvalidCardId;
    }
    game.RoundPlayerCardIds.Last()[playerIndex].Add(cardId);
    do {
      game = _IncrementActivePlayer(game);
    } while (!game.PlayerCards[game.ActivePlayerIndex].Any(x => x.State == CardState.InPlay));
    return game;
  }
  
  public static Result<Game, GameTurnError> TurnPlaceBid(Game game, Guid playerId, int bid) {
    int playerIndex = Array.IndexOf(game.PlayerIds, playerId);
    if (playerIndex == -1 || playerIndex != game.ActivePlayerIndex) {
      return GameTurnError.InvalidPlayerId;
    }
    if (game.RoundPlayerCardIds.Last().Count(x => x.Count() > 0) < game.PlayerIds.Length - game.PlayerCards.Count(y => y.All(z => z.State == CardState.Discarded))) {
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
    do {
      game = _IncrementActivePlayer(game);
    } while (game.RoundBids.Last()[game.ActivePlayerIndex] == -1 || !game.PlayerCards[game.ActivePlayerIndex].Any(x => x.State == CardState.InPlay));
    if (game.RoundBids.Last().Count(x => x == GameFunctions.SKIP_BIDDING_VALUE) == game.PlayerIds.Length - 1 - game.PlayerCards.Count(x => x.Count(y => y.State == CardState.Discarded) == x.Length)){
      game.ActivePlayerIndex = Array.IndexOf(game.RoundBids.Last(), game.RoundBids.Last().Max());
    }
    return game;
  }

  private static Game _AddNewRoundOfPlay(Game game) {
    game.RoundPlayerCardIds.Add(_GenerateRoundPlayerCardIds(game.PlayerIds.Length));
    game.RoundBids.Add(new int[game.PlayerIds.Length]);
    game.RoundRevealedCardPlayerIndexes.Add(new List<int>());
    return game;
  }

  public static Result<Game, GameTurnError> TurnFlipCard(Game game, Guid playerId, int targetPlayerIndex) {
    if (game.GameComplete) {
      return GameTurnError.GameHasFinished;
    }
    int playerIndex = Array.IndexOf(game.PlayerIds, playerId);
    if (playerIndex == -1 || playerIndex != game.ActivePlayerIndex) {
      return GameTurnError.InvalidPlayerId;
    }
    var currentRoundCards = game.RoundPlayerCardIds.Last();
    if (targetPlayerIndex != playerIndex && 
      currentRoundCards[playerIndex].Count() != game.RoundRevealedCardPlayerIndexes.Last().Count(x => x == playerIndex)) {
      return GameTurnError.MustRevealAllOwnCardsFirst;
    }
    if (game.RoundRevealedCardPlayerIndexes.Last().Count(x => x == targetPlayerIndex) >= currentRoundCards[targetPlayerIndex].Count()) {
      return GameTurnError.NoCardsLeftToFlip;
    }
    game.RoundRevealedCardPlayerIndexes.Last().Add(targetPlayerIndex);
    var cardIndexOfStack = game.RoundRevealedCardPlayerIndexes.Last().Count(x => x == targetPlayerIndex) - 1;
    var cardId = game.RoundPlayerCardIds.Last()[targetPlayerIndex][cardIndexOfStack];
    bool cardWasSkull = game.PlayerCards[targetPlayerIndex].First(x => x.Id == cardId).Type == CardType.Skull;
    
    if (cardWasSkull) {
      // This is hideous, but random often is.
      int startingCards = game.PlayerCards[playerIndex].Where(x => x.State != CardState.Discarded).Count();
      if (startingCards > 1) {
        do { 
          int cardToLose = new Random().Next(0, game.PlayerCards[playerIndex].Count() - 1); 
          game.PlayerCards[playerIndex][cardToLose].State = CardState.Discarded;
        }
        while (startingCards == game.PlayerCards[playerIndex].Where(x => x.State != CardState.Discarded).Count());
      } else {
        game.PlayerCards[playerIndex] = game.PlayerCards[playerIndex].Select(x => {
          x.State = CardState.Discarded;
          return x;
        }).ToArray();
      }
      game.ActivePlayerIndex = targetPlayerIndex;
      while (!game.PlayerCards[game.ActivePlayerIndex].Any(x => x.State == CardState.InPlay)) {
        game = _IncrementActivePlayer(game);
      }

      if (game.PlayerCards.Count(x => !x.All(y => y.State == CardState.Discarded)) == 1) {
        game.GameComplete = true;
      } else {
        game = _AddNewRoundOfPlay(game);
      }
    }

    if (!cardWasSkull && game.RoundBids.Last().Max() == game.RoundRevealedCardPlayerIndexes.Last().Count()) {
      game.ActivePlayerIndex = playerIndex;
      game.RoundWinPlayerIndexes.Add(playerIndex);
      if (game.RoundWinPlayerIndexes.Count(x => x == playerIndex) == ROUNDS_TO_WIN){
        game.GameComplete = true;
      } else {
        game = _AddNewRoundOfPlay(game);
      }
    }
    return game;
  }
}