import { readable } from 'svelte/store';

import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { API_ROOT_URL } from '../config';

const newPlayerConnection = new HubConnectionBuilder()
.withUrl(API_ROOT_URL + '/player/ws')
.build()

export const playerConnection = readable<HubConnection>(newPlayerConnection, function start(set) {
    // const playerSuccessHandler = (msg: any) => {
    //   if (msg.activity === "Subscribed" && msg.id === USER_ID) {
    //     setPlayerConnection(newPlayerConnection)
    //     setError(false)
    //     setLoading(false)
    //   }
    // }

    // const newGameHandler = (msg: any) => {
    //   if (msg.activity === "GameCreated") {
    //     setNewGameId(msg.id);
    //   }
    // }

    // newPlayerConnection.on("send", newGameHandler)

    // newPlayerConnection.on("send", playerSuccessHandler)

  newPlayerConnection.start()
      //.then(() => newPlayerConnection.send("subscribeToUserId", USER_ID, USER_SECRET))
      //.then(() => newPlayerConnection.send("updateNickname", USER_ID, USER_SECRET, localStorage.getItem(USER_NICKNAME_IDENTIFIER)))
      //.catch(() => setError(true))
	set(newPlayerConnection)

	return function stop() { newPlayerConnection && newPlayerConnection.stop() };
});

export const playerConnectionState = readable<string>(newPlayerConnection.state, function start(set) {
  const updater = setInterval(() => set(newPlayerConnection.state), 500)
  return function stop() {clearInterval(updater)};
});
