import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import React from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "react-query";
import App from "./App";
import { msalConfig } from "./Authentication/authConfig";

const msalInstance = new PublicClientApplication(msalConfig);
const client = new QueryClient();

async function main() {
  await msalInstance.initialize();

  msalInstance
    .handleRedirectPromise()
    .then((response) => {
      if (response && response.account) {
        // user just came back from login
        msalInstance.setActiveAccount(response.account);

        // ⤵ redirect them to your admin page
        window.history.replaceState({}, document.title, "/admin");
      } else {
        // returning user: restore from cache
        const accounts = msalInstance.getAllAccounts();
        if (accounts.length > 0) {
          msalInstance.setActiveAccount(accounts[0]);
        }
      }

      renderApp();
    })
    .catch((error) => {
      console.error("MSAL redirect error:", error);
      renderApp();
    });
}

function renderApp() {
  const rootElement = document.getElementById("root");
  if (rootElement) {
    const root = createRoot(rootElement);
    root.render(
      <QueryClientProvider client={client}>
        <MsalProvider instance={msalInstance}>
          <App />
        </MsalProvider>
      </QueryClientProvider>
    );
  }
}

main().catch((err) => console.error("MSAL init error:", err));
