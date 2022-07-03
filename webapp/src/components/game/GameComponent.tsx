import { useParams } from "react-router-dom";

export function GameComponent() {

  const { gameId } = useParams<{ gameId: string }>();

  return <></>
}