import { v4 as randomUUID } from "uuid";

export const USER_ID_IDENTIFIER = "Openskull_userid";

const storageUserId = localStorage.getItem(USER_ID_IDENTIFIER)
const userId = storageUserId || randomUUID()
if (!storageUserId) {
  localStorage.setItem(USER_ID_IDENTIFIER, userId)
}

export const USER_SECRET_IDENTIFIER = "Openskull_usersecret";

const storageUserSecret = localStorage.getItem(USER_SECRET_IDENTIFIER)
const userSecret = storageUserSecret || randomUUID()
if (!storageUserSecret) {
  localStorage.setItem(USER_SECRET_IDENTIFIER, userSecret)
}

export const USER_NICKNAME_IDENTIFIER = "Openskull_usernickname";

const storageUserNickname = localStorage.getItem(USER_NICKNAME_IDENTIFIER)
const userNickname = storageUserNickname || "New Player"
if (!storageUserNickname) {
  localStorage.setItem(USER_NICKNAME_IDENTIFIER, userNickname)
}

export const USER_ID_HEADER = "X-OpenSkull-UserId"
export const USER_ID = userId
export const USER_SECRET = userSecret
export const USER_NICKNAME = userNickname
export const API_ROOT_URL = process.env.REACT_APP_API_ROOT_URL ?? "http://localhost:5248";