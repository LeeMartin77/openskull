import FlowerIcon from '@mui/icons-material/LocalFlorist';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';
import CircleIcon from '@mui/icons-material/Circle';
import SkullIcon from '@mui/icons-material/Balcony';
import { CardType } from '../../../models/Game';

export interface ICardDisplayIcons {
  playerRevealedCards: CardType[],
  unrevealedCardsPlayed: number,
  cardsUnplayed: number,
  cardsLost: number
}

export function CardDisplayIconsComponent({ playerRevealedCards, unrevealedCardsPlayed, cardsUnplayed, cardsLost }: ICardDisplayIcons) {
  const iconStyle = {width: "1.5em", height: "1.5em"};
  const playerRevealedCardIcons = playerRevealedCards.map((card, i) => card === CardType.Flower ? <FlowerIcon style={iconStyle} key={"revealed-"+i} /> : <SkullIcon style={iconStyle} key={"revealed-"+i} />);

  return <>
    {playerRevealedCardIcons}
    {Array.from(Array(unrevealedCardsPlayed).keys()).map(i => <CircleIcon style={iconStyle} key={"hidden-"+i}/>)}
    {Array.from(Array(cardsUnplayed).keys()).map(i => <CircleIcon style={iconStyle}  key={"unplayed-"+i} color="disabled" />)}
    {Array.from(Array(cardsLost).keys()).map(i => <LostCardIcon style={iconStyle} key={"lost-"+i} color="disabled" />)}
  </>
}
