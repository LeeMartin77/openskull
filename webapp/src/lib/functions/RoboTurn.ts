import { SKIP_VALUE } from "src/config";
import { generateUserHeaders } from "src/stores/player";
import type { SoloGameBot } from "src/stores/solo";
import { CardState, RoundPhase, type PlayerGame } from "src/types/Game";
import { playCard, placeBid, flipCard } from './TurnFunctions';


function getPossibleCardIds(game: PlayerGame): string[] {
  return game.playerCards.reduce((availableIds, card) => {
    if (card.state !== CardState.Discarded && !game.playerRoundCardIdsPlayed[game.roundNumber - 1].includes(card.id)) {
      return [...availableIds, card.id]
    }
    return availableIds;
  }, []);
}
function playRandomCard(game: PlayerGame, botHeaders: {[key: string]: string}) {
  const possibleCardIds = getPossibleCardIds(game);
  if (possibleCardIds.length === 0){
    return placeBid(game.id, 1, botHeaders)
  }
  return playCard(game.id, possibleCardIds[Math.floor(Math.random() * possibleCardIds.length)], botHeaders)
}

export async function playRoboTurn(bot: SoloGameBot, game: PlayerGame): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 500 + (Math.floor(Math.random() * 3) * 1000)));
  const botHeaders = generateUserHeaders(bot.id, bot.secret)
  switch(game.currentRoundPhase) {
    case RoundPhase.PlayFirstCards:
      playRandomCard(game, botHeaders);
    case RoundPhase.PlayCards:
      const cardsAvailable = getPossibleCardIds(game).length;
      if (cardsAvailable > 0 && Math.floor(Math.random() * (cardsAvailable + 1)) > 0) {
        return playRandomCard(game, botHeaders);
      }
      return placeBid(game.id, Math.floor(Math.random() * game.roundCountPlayerCardsPlayed[game.roundNumber - 1].reduce((prev, curr) => prev + curr, 0) / 2) + 1, botHeaders);
    case RoundPhase.Bidding:
      const botBid = Math.floor(Math.random() * game.roundCountPlayerCardsPlayed[game.roundNumber - 1].reduce((prev, curr) => prev + curr, 0)) + 1;
      const minBid = Math.max(...game.currentBids) + 1;
      return placeBid(game.id, botBid >= minBid ? botBid : SKIP_VALUE, botHeaders)
    case RoundPhase.Flipping:
      if (game.roundPlayerCardsRevealed[game.roundNumber - 1][game.activePlayerIndex].length !== game.roundCountPlayerCardsPlayed[game.roundNumber - 1][game.activePlayerIndex]) {
        // Own cards left
        return flipCard(game.id, game.activePlayerIndex, botHeaders);
      }
      const flippable = game.roundCountPlayerCardsPlayed[game.roundNumber - 1]
          .map((playedCount, i) => [playedCount, i])
          .filter(([playedCount, playerIndex]) => playerIndex !== game.activePlayerIndex && playedCount > game.roundPlayerCardsRevealed[game.roundNumber - 1][playerIndex].length)
      return flipCard(game.id, flippable[Math.floor(Math.random() * flippable.length)][1], botHeaders);
  }
}