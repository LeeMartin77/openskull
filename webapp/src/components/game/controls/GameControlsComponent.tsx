import { useEffect, useState } from "react";
import { PlayerGame, RoundPhase } from "../../../models/Game";
import { GameFlipCardControlComponent } from "./GameFlipCardControlComponent";
import { GamePlaceBidControlComponent } from "./GamePlaceBidControlComponent";
import { GamePlayCardControlComponent } from "./GamePlayCardControlComponent";

export function GameControlsComponent({ game }: { game: PlayerGame }) {
  
  const [clicked, setClicked] = useState(false);
  const setStateGame = useState(game)[1];
  
  // Bit of a hack. Last updated isn't perfect but should
  // change enough to trigger this
  useEffect(() => {
    setStateGame(prev => {
      if (prev.lastUpdated != game.lastUpdated) {
        setClicked(false)
      }
      return game;
    })
  }, [game, setStateGame, setClicked])

  if (game.currentRoundPhase !== RoundPhase.Flipping) {
    return <>
      {game.currentRoundPhase !== RoundPhase.Bidding && <div style={{ marginBottom: "1em" }}><GamePlayCardControlComponent game={game} clicked={clicked} setClicked={setClicked}/></div>}
      {game.currentRoundPhase !== RoundPhase.PlayFirstCards && <div><GamePlaceBidControlComponent game={game} clicked={clicked} setClicked={setClicked}/></div>}
    </>
  }
  if (game.activePlayerIndex === game.playerIndex) {
    return <GameFlipCardControlComponent game={game} clicked={clicked} setClicked={setClicked}/>;
  }
  const cardsRevealed = game.roundPlayerCardsRevealed[game.roundNumber - 1].reduce((countSoFar, playerCards)=> countSoFar + playerCards.length, 0)
  const bid = Math.max(...game.currentBids)
  return <>Waiting for {game.playerNicknames[game.activePlayerIndex]} to flip {bid - cardsRevealed} more card(s)...</>
}