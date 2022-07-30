export type OpenskullMessage = {
  id: string;
  activity: string;
  details: { gameSize: number, queueSize: number }
  roomDetails: { roomId: string, playerNicknames: string[] }
}