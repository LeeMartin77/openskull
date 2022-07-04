import { v4 as randomUUID } from "uuid";

export const USER_ID_IDENTIFIER = "Openskull_userid";

const storageUserId = localStorage.getItem(USER_ID_IDENTIFIER)
const userId = storageUserId || randomUUID()
if (!storageUserId) {
  localStorage.setItem(USER_ID_IDENTIFIER, userId)
}
export const USER_ID_HEADER = "X-OpenSkull-UserId"
export const USER_ID = userId
export const API_ROOT_URL = process.env.REACT_APP_API_ROOT_URL ?? "http://localhost:5248";