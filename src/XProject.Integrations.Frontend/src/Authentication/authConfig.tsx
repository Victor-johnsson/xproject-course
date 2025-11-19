import { LogLevel, Configuration } from "@azure/msal-browser";

const getRedirectUri = () => {
  if (import.meta.env.DEV) {
    const port = window.location.port || "3000";
    return `http://localhost:${port}/`;
  }
  return import.meta.env.VITE_AZURE_REDIRECT_URI!;
};
export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID!,
    authority: import.meta.env.VITE_AZURE_AUTHORITY,
    redirectUri: getRedirectUri(),
    postLogoutRedirectUri:
      import.meta.env.VITE_AZURE_POST_LOGOUT_REDIRECT_URI || "/",
    navigateToLoginRequestUrl: false,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (
        level: LogLevel,
        message: string,
        containsPii: boolean,
      ): void => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Info:
            console.info(message);
            return;
          case LogLevel.Verbose:
            console.debug(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
          default:
            return;
        }
      },
    },
  },
};

console.log("clientid", msalConfig.auth.clientId);
console.log("authority", msalConfig.auth.authority);
console.log("redirectUri", msalConfig.auth.redirectUri);
console.log("postLogout", msalConfig.auth.postLogoutRedirectUri);
export const loginRequest = {
  scopes: [import.meta.env.VITE_AZURE_SCOPE!],
  prompt: "select_account",
};
