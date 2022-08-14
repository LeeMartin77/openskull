import { writable } from "svelte/store";

const OPENSKULL_SOLOGAME_IDENTIFIER = "Openskull_sologameid";

export const CURRENT_SOLO_GAME = writable<string | null>(
  localStorage.getItem(OPENSKULL_SOLOGAME_IDENTIFIER) ?? null
);

CURRENT_SOLO_GAME.subscribe((newSoloGameId: string | null) => {
  if (newSoloGameId == null) {
    localStorage.removeItem(OPENSKULL_SOLOGAME_IDENTIFIER)
  } else {
    localStorage.setItem(OPENSKULL_SOLOGAME_IDENTIFIER, newSoloGameId)
  }
}
);

const OPENSKULL_SOLOGAME_BOTS_IDENTIFIER = "Openskull_sologamebots";

export interface SoloGameBot {
  id: string,
  secret: string,
  nickname: string
}

export const SOLOGAME_BOTS = writable<SoloGameBot[]> (
  localStorage.getItem(OPENSKULL_SOLOGAME_BOTS_IDENTIFIER) ? JSON.parse(localStorage.getItem(OPENSKULL_SOLOGAME_BOTS_IDENTIFIER)) : []
);

SOLOGAME_BOTS.subscribe((newBots: SoloGameBot[]) => 
    localStorage.setItem(OPENSKULL_SOLOGAME_BOTS_IDENTIFIER, JSON.stringify(newBots))
);