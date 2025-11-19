import React from "react";
import { Box, Alert, Button } from "@mui/material";

interface UnauthorizedComponentProps {
  error: string;
  onLogout: () => void;
}

const UnauthorizedComponent: React.FC<UnauthorizedComponentProps> = ({
  error,
  onLogout,
}) => (
  <Box
    display="flex"
    flexDirection="column"
    alignItems="center"
    bgcolor="#FDEDED"
    p={2}
  >
    <Alert severity="error" sx={{ mb: 2 }}>
      {error}
    </Alert>
    <Button variant="contained" color="error" onClick={onLogout}>
      Log Out
    </Button>
  </Box>
);

export default UnauthorizedComponent;
