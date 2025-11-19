import React from "react";
import { Grid, Card, CardContent, Box } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import Item from "./Item";
import { CartItemType } from "../Services/service";
import "../styles.css";

interface Props {
  products: CartItemType[];
  onAction: (item: CartItemType) => void;
  actionLabel: string;
  isAdmin?: boolean;
  onAddProduct?: () => void;
}

export default function ProductList({
  products,
  onAction,
  actionLabel,
  isAdmin = false,
  onAddProduct,
}: Props) {
  return (
    <Grid container spacing={3}>
      {isAdmin && (
        <Grid item xs={12} sm={4}>
          <Card
            sx={{
              display: "flex",
              flexDirection: "column",
              justifyContent: "center",
              alignItems: "center",
              width: "100%",
              borderRadius: "20px",
              height: "100%",
              transition: "0.3s",
              boxShadow: "0px 4px 10px rgba(0, 0, 0, 0.2)",
              "&:hover": {
                transform: "scale(1.02)",
                boxShadow: "0px 6px 15px rgba(0, 0, 0, 0.3)",
              },
              cursor: "pointer",
            }}
            onClick={onAddProduct}
          >
            <CardContent>
              <Box display="flex" justifyContent="center" alignItems="center">
                <AddIcon
                  fontSize="large"
                  sx={{ color: "#1976D2", fontSize: "3rem" }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      )}

      {products.map((item) => (
        <Grid item key={item.id} xs={12} sm={4}>
          <Item
            key={item.id}
            item={item}
            onAction={onAction}
            actionLabel={actionLabel}
          />
        </Grid>
      ))}
    </Grid>
  );
}
