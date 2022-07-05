export enum CardType {
  Flower,
  Skull
}

export enum CardState {
  InPlay,
  Discarded
}


export enum TurnAction {
  Card,
  Bid,
  Flip
}

export interface Card {
    id: string;
    type: CardType;
    state: CardState;
}

export interface PublicGame 
{
  id: string;
  activePlayerIndex: number;
  playerCount: number;
  playerCardStartingCount: number;
  roundNumber: number;
  currentCountPlayerCardsAvailable: number[];
  currentBids: number[];
  roundCountPlayerCardsPlayed: number[][];
  roundPlayerCardsRevealed: CardType[][][];
  roundWinners: number[];
  gameComplete: boolean;
}

export interface PlayerGame extends PublicGame
{
  playerId: string;
  playerIndex: number;
  playerCards: Card[];
  playerRoundCardIdsPlayed: string[][];
}
