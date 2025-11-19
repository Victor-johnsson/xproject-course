import React from "react";
import {
  Button,
  Card,
  CardContent,
  CardMedia,
  Typography,
  Box,
} from "@mui/material";
import { CartItemType } from "../Services/service";
import "../styles.css";

interface Props {
  item: CartItemType;
  addToCart: (clickedItem: CartItemType) => void;
  removeFromCart: (id: string) => void;
}

export default function CartItem({ item, addToCart, removeFromCart }: Props) {
  return (
    <Card
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        borderBottom: "1px solid lightblue",
        padding: "16px",
        transition: "0.3s",
        "&:hover": {
          boxShadow: "0px 4px 12px rgba(0, 0, 0, 0.2)",
        },
      }}
    >
      <CardContent
        sx={{
          flex: 1,
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        }}
      >
        <Typography variant="h6" sx={{ fontWeight: "bold" }}>
          {item.name}
        </Typography>

        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            marginTop: "8px",
          }}
        >
          <Typography variant="body1">
            Price: <strong>${item.price}</strong>
          </Typography>
          <Typography variant="body1">
            Total: <strong>${(item.amount * item.price).toFixed(2)}</strong>
          </Typography>
        </Box>

        <Box sx={{ display: "flex", alignItems: "center", gap: "10px", mt: 2 }}>
          <Button
            size="small"
            variant="contained"
            color="error"
            onClick={() => removeFromCart(item.id)}
            sx={{
              minWidth: "32px",
              fontSize: "1rem",
              fontWeight: "bold",
            }}
          >
            -
          </Button>
          <Typography variant="body1" sx={{ fontWeight: "bold" }}>
            {item.amount}
          </Typography>
          <Button
            size="small"
            variant="contained"
            color="primary"
            onClick={() => addToCart(item)}
            sx={{
              minWidth: "32px",
              fontSize: "1rem",
              fontWeight: "bold",
            }}
          >
            +
          </Button>
        </Box>
      </CardContent>

      <CardMedia
        component="img"
        image={item.imageUrl}
        alt={item.name}
        sx={{
          width: "90px",
          height: "90px",
          objectFit: "cover",
          borderRadius: "8px",
          marginLeft: "20px",
        }}
      />
    </Card>
  );
}
