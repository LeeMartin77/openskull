import { API_ROOT_URL } from "src/config";
import { generateUserHeaders } from "src/stores/player";
import { TurnAction } from "src/types/Game";

// TODO: Backfill these into actual game

export const playCard = (gameId: string, cardId: string, headers?: {[key: string]:string}) => {
  fetch(`${API_ROOT_URL}/games/${gameId}/turn`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(headers ? headers : generateUserHeaders())},
    body: JSON.stringify({
      action: TurnAction[TurnAction.Card],
      cardId
    })
  });
};

export const placeBid = (gameId: string, bid: number, headers?: {[key: string]:string}) => {
  fetch(`${API_ROOT_URL}/games/${gameId}/turn`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(headers ? headers : generateUserHeaders()) },
    body: JSON.stringify({
      action: TurnAction[TurnAction.Bid],
      bid
    })
  });
};

export const flipCard = (gameId: string, targetPlayerIndex: number, headers?: {[key: string]:string}) => {
  fetch(`${API_ROOT_URL}/games/${gameId}/turn`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(headers ? headers : generateUserHeaders()) },
    body: JSON.stringify({
      action: TurnAction[TurnAction.Flip],
      targetPlayerIndex
    })
  });
};