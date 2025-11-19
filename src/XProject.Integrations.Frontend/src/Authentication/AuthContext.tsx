import React from "react";

const AuthContext = React.createContext({
  handleLogout: () => {},
});

export default AuthContext;
