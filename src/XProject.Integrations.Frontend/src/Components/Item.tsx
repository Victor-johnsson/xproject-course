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
  onAction: (clickedItem: CartItemType) => void;
  actionLabel: string;
}

export default function Item({ item, onAction, actionLabel }: Props) {
  return (
    <Card
      sx={{
        display: "flex",
        flexDirection: "column",
        justifyContent: "space-between",
        width: "100%",
        borderRadius: "20px",
        height: "100%",
        transition: "0.3s",
        boxShadow: "0px 4px 10px rgba(0, 0, 0, 0.2)",
        "&:hover": {
          transform: "scale(1.02)",
          boxShadow: "0px 6px 15px rgba(0, 0, 0, 0.3)",
        },
      }}
    >
      <CardMedia
        component="img"
        image={item.imageUrl}
        alt={item.name}
        sx={{
          maxHeight: "250px",
          objectFit: "cover",
          borderRadius: "20px 20px 0 0",
        }}
      />

      <CardContent
        sx={{
          flexGrow: 1,
          fontFamily: "Arial, Helvetica, sans-serif",
          padding: "1rem",
          textAlign: "center",
        }}
      >
        <Typography variant="h6" sx={{ fontWeight: "bold" }}>
          {item.name}
        </Typography>
        <Typography variant="h6" color="primary">
          ${item.price}
        </Typography>
      </CardContent>

      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          paddingBottom: "1rem",
        }}
      >
        <Button
          variant="contained"
          onClick={() => onAction(item)}
          sx={{
            margin: "1rem",
            borderRadius: "10px",
            transition: "all 0.3s ease",
            backgroundColor:
              actionLabel === "Add to Cart" ? "#1976D2" : "#D32F2F",
            color: "white",
            "&:hover": {
              backgroundColor:
                actionLabel === "Add to Cart" ? "#135BA1" : "#B71C1C",
            },
          }}
        >
          {actionLabel}
        </Button>
      </Box>
    </Card>
  );
}
