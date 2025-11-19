import React from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./Pages/Webshop";
import AdminWebshop from "./Pages/Admin";
import { AuthProvider } from "./Authentication/hooks/auth-provider";
import ProtectedRoute from "./Authentication/ProtectedRoute";
import AuthContext from "./Authentication/AuthContext";
import useAuth from "./Authentication/hooks/useAuth";
import UnauthorizedComponent from "./Components/UnauthorizedComponent";

const App = () => {
  return (
    <AuthProvider>
      <AuthConsumerApp />
    </AuthProvider>
  );
};

const AuthConsumerApp = () => {
  const { status, handleLogout, token } = useAuth();

  if (status.error) {
    return (
      <UnauthorizedComponent error={status.error} onLogout={handleLogout} />
    );
  }

  return (
    <AuthContext.Provider value={{ handleLogout }}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route element={<ProtectedRoute />}>
            <Route path="/admin" element={<AdminWebshop token={token} />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthContext.Provider>
  );
};

export default App;
