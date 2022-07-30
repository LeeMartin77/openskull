export const API_ROOT_URL = import.meta.env.VITE_API_ROOT_URL;
export const USER_ID_HEADER = 'X-OpenSkull-UserId';
export const USER_SECRET_HEADER = 'X-OpenSkull-UserSecret';
export const generateUserHeaders = (userId: string, userSecret: string) => {
  return {
    [USER_ID_HEADER]: userId,
    [USER_SECRET_HEADER]: userSecret
  };
};
