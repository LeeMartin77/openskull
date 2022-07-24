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

export enum RoundPhase {
  PlayFirstCards,
  PlayCards,
  Bidding,
  Flipping
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
  playerNicknames: string[];
  playerCardStartingCount: number;
  roundNumber: number;
  currentCountPlayerCardsAvailable: number[];
  currentBids: number[];
  currentRoundPhase: RoundPhase;
  roundCountPlayerCardsPlayed: number[][];
  roundPlayerCardsRevealed: CardType[][][];
  roundWinners: number[];
  gameComplete: boolean;
  lastUpdated: Date;
}

export interface PlayerGame extends PublicGame
{
  playerId: string;
  playerIndex: number;
  playerCards: Card[];
  playerRoundCardIdsPlayed: string[][];
}
