import { SKIP_VALUE } from "src/config";
import { generateUserHeaders } from "src/stores/player";
import type { SoloGameBot } from "src/stores/solo";
import { CardState, RoundPhase, type PlayerGame } from "src/types/Game";
import { playCard, placeBid, flipCard } from './TurnFunctions';

export async function playRoboTurn(bot: SoloGameBot, game: PlayerGame): Promise<void> {
  // KISS to start:
  // Play Random Card if playing cards
  // If no cards and no bidding yet, bid lowest
  // If there is a bid, withdraw
  // If active index and flipping, flip appropriately
  await new Promise((resolve) => setTimeout(resolve, 500 + (Math.floor(Math.random() * 3) * 1000)));
  const botHeaders = generateUserHeaders(bot.id, bot.secret)
  switch(game.currentRoundPhase) {
    case RoundPhase.PlayFirstCards:
    case RoundPhase.PlayCards:
      const possibleCardIds = game.playerCards.reduce((availableIds, card) => {
        if (card.state !== CardState.Discarded && !game.playerRoundCardIdsPlayed[game.roundNumber - 1].includes(card.id)) {
          return [...availableIds, card.id]
        }
        return availableIds;
      }, [])
      if (possibleCardIds.length === 0){
        return placeBid(game.id, 1, botHeaders)
      }
      return playCard(game.id, possibleCardIds[Math.floor(Math.random() * possibleCardIds.length)], botHeaders)
    case RoundPhase.Bidding:
      return placeBid(game.id, SKIP_VALUE, botHeaders)
    case RoundPhase.Flipping:
      // TODO: this is going to be a pain
      return;
  }
}