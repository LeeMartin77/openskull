import { readable, writable } from 'svelte/store';

import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { API_ROOT_URL } from 'src/config';
import { v4 as randomUUID } from 'uuid';

const newPlayerConnection = new HubConnectionBuilder()
.withUrl(API_ROOT_URL + '/player/ws')
.build()


const getOrStoreNewVariable = (key: string, generated: string) => {
  let lstUserID = localStorage.getItem(key)
  if (!lstUserID) {
    lstUserID = generated
    localStorage.setItem(OPENSKULL_USER_ID_KEY, lstUserID)
  }
  return lstUserID;
}

const OPENSKULL_USER_ID_KEY = 'Openskull_userid'
export const OPENSKULL_USER_ID = getOrStoreNewVariable(OPENSKULL_USER_ID_KEY, randomUUID())

const OPENSKULL_USER_SECRET_KEY = 'Openskull_usersecret'
export const OPENSKULL_USER_SECRET = getOrStoreNewVariable(OPENSKULL_USER_SECRET_KEY, randomUUID())

const OPENSKULL_USER_NICKNAME_IDENTIFIER = "Openskull_usernickname";
export const OPENSKULL_USER_NICKNAME = writable<string>(localStorage.getItem(OPENSKULL_USER_NICKNAME_IDENTIFIER))

OPENSKULL_USER_NICKNAME.subscribe(newNickname => localStorage.setItem(OPENSKULL_USER_NICKNAME_IDENTIFIER, newNickname))

export const playerConnection = readable<HubConnection>(newPlayerConnection, function start(set) {
  const playerSuccessHandler = (msg: any) => {
    if (msg.activity === "Subscribed" && msg.id === OPENSKULL_USER_ID) {
      set(newPlayerConnection)
    }
  }

  newPlayerConnection.on("send", playerSuccessHandler)

  newPlayerConnection.start()
      .then(() => newPlayerConnection.send("subscribeToUserId", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET))

	return function stop() { newPlayerConnection && newPlayerConnection.stop() };
});

export const playerConnectionState = readable<string>(newPlayerConnection.state, function start(set) {
  // This is just because signalr doesn't provide a nice "on state change" listener
  const updater = setInterval(() => set(newPlayerConnection.state), 500)
  return function stop() {clearInterval(updater)};
});
