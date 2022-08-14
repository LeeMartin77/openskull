import type { PlayerGame } from "src/types/Game";
import { playCard, placeBid, flipCard } from './TurnFunctions';

export function playRoboTurn(game: PlayerGame) {
  // KISS to start:
  // Play Random Card if playing cards
  // If no cards and no bidding yet, bid lowest
  // If there is a bid, withdraw
  // If active index and flipping, flip appropriately
}