import { useState, useEffect, useReducer } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";
import { validateToken } from "../../Services/service";
import { loginRequest } from "../authConfig";

interface AuthState {
  loading: boolean;
  authorized: boolean;
  error: string | null;
}

type AuthAction =
  | { type: "SET_LOADING"; payload: boolean }
  | { type: "SET_AUTHORIZED"; payload: boolean }
  | { type: "SET_ERROR"; payload: string | null };

const initialState: AuthState = {
  loading: true,
  authorized: false,
  error: null,
};

function reducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case "SET_LOADING":
      return { ...state, loading: action.payload };
    case "SET_AUTHORIZED":
      return { ...state, authorized: action.payload };
    case "SET_ERROR":
      return { ...state, error: action.payload };
    default:
      return state;
  }
}

interface UseAuthReturn {
  status: AuthState;
  token: string;
  handleLogout: () => void;
}

function useAuth(): UseAuthReturn {
  const { instance, inProgress } = useMsal();
  const [state, dispatch] = useReducer(reducer, initialState);
  const [token, setToken] = useState<string>("");

  useEffect(() => {
    let isMounted = true;

    const acquireAndValidateToken = async () => {
      try {
        const activeAccount = instance.getActiveAccount();
        if (!activeAccount) {
          // Do not redirect here—leave it for ProtectedRoute
          return;
        }
        const response = await instance.acquireTokenSilent({
          ...loginRequest,
        });
        if (!isMounted) return;

        const acquiredToken = response.accessToken;
        setToken(acquiredToken);

        // const isValid = await validateToken(acquiredToken);
        const isValid = true;
        if (isMounted) {
          if (isValid) {
            dispatch({ type: "SET_LOADING", payload: false });
            dispatch({ type: "SET_AUTHORIZED", payload: true });
            dispatch({ type: "SET_ERROR", payload: null });
          } else {
            dispatch({ type: "SET_LOADING", payload: false });
            dispatch({ type: "SET_AUTHORIZED", payload: false });
            dispatch({
              type: "SET_ERROR",
              payload:
                "Unauthorized access. Please click on the LOG OUT button to log out and try again.",
            });
          }
        }
      } catch (error) {
        if (!isMounted) return;
        console.error("Token acquisition error:", error);
        dispatch({ type: "SET_LOADING", payload: false });
        dispatch({ type: "SET_AUTHORIZED", payload: false });
        dispatch({
          type: "SET_ERROR",
          payload:
            error instanceof Error
              ? error.message
              : "Token acquisition failed.",
        });
      }
    };

    if (inProgress === InteractionStatus.None && instance.getActiveAccount()) {
      acquireAndValidateToken();
    }

    return () => {
      isMounted = false;
    };
  }, [instance, inProgress]);

  const handleLogout = () => {
    instance
      .logoutRedirect({
        onRedirectNavigate: () => {
          instance.setActiveAccount(null);
          return true;
        },
      })
      .catch((error: unknown) => {
        console.error("Logout failed:", error);
      });
  };

  return { status: state, token, handleLogout };
}

export default useAuth;
