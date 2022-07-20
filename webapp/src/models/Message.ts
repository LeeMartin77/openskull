
export interface IOpenskullMessage 
{
  id: string;
  activity: string;
  details: { gameSize: number, queueSize: number }
}