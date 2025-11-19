import React from "react";
import { AppBar, Toolbar, Typography, IconButton, Badge } from "@mui/material";
import { AddShoppingCart } from "@mui/icons-material";
import LogoutIcon from "@mui/icons-material/Logout";
import "../styles.css";

interface AppBarProps {
  title: string;
  logoutButton?: boolean;
  showCart?: boolean;
  cartItemCount?: number;
  onLogoutClick?: () => void;
  onCartClick?: () => void;
}

export default function AppBarComponent({
  title,
  logoutButton = false,
  showCart = false,
  cartItemCount = 0,
  onLogoutClick,
  onCartClick,
}: AppBarProps) {
  return (
    <AppBar position="fixed" sx={{ backgroundColor: "#1976D2" }}>
      <Toolbar sx={{ display: "flex", justifyContent: "space-between" }}>
        <Typography variant="h6" sx={{ fontWeight: "bold", color: "white" }}>
          {title}
        </Typography>

        {showCart && (
          <IconButton onClick={onCartClick} sx={{ color: "white" }}>
            <Badge badgeContent={cartItemCount} color="error">
              <AddShoppingCart />
            </Badge>
          </IconButton>
        )}
        {logoutButton && (
          <IconButton onClick={onLogoutClick} sx={{ color: "white" }}>
            <LogoutIcon />
          </IconButton>
        )}
      </Toolbar>
    </AppBar>
  );
}
