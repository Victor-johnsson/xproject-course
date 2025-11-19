import React, { useState } from "react";
import {
  Box,
  Container,
  Grid,
  Pagination,
  LinearProgress,
} from "@mui/material";
import { useQuery } from "react-query";
import { CartItemType, getProducts } from "../Services/service";
import AppBarComponent from "../Components/AppBarComponent";
import ProductList from "../Components/ProductList";
import Cart from "../Components/Cart";
import "../styles.css";

export default function Webshop() {
  const { data, isLoading, error } = useQuery<CartItemType[]>(
    ["products"],
    getProducts,
  );
  const [cartOpen, setCartOpen] = useState(false);
  const [cartItems, setCartItems] = useState<CartItemType[]>([]);
  const [page, setPage] = useState(1);
  const itemsPerPage = 6;

  if (isLoading) return <LinearProgress />;
  if (error) return <div>Something went wrong</div>;

  const paginatedData =
    data?.slice((page - 1) * itemsPerPage, page * itemsPerPage) || [];

  const handleAddToCart = (clickedItem: CartItemType) => {
    setCartItems((prev) => {
      const isItemInCart = prev.find((item) => item.id === clickedItem.id);
      if (isItemInCart) {
        return prev.map((item) =>
          item.id === clickedItem.id
            ? { ...item, amount: item.amount + 1 }
            : item,
        );
      }
      return [...prev, { ...clickedItem, amount: 1 }];
    });
  };

  const handleRemoveFromCart = (id: string) => {
    setCartItems((prev) =>
      prev.reduce((acc, item) => {
        if (item.id === id) {
          if (item.amount === 1) return acc;
          return [...acc, { ...item, amount: item.amount - 1 }];
        } else {
          return [...acc, item];
        }
      }, [] as CartItemType[]),
    );
  };

  const handleClearCart = () => {
    setCartItems([]);
  };

  return (
    <Box>
      <AppBarComponent
        title="WebshopX"
        showCart
        cartItemCount={cartItems.reduce((acc, item) => acc + item.amount, 0)}
        onCartClick={() => setCartOpen(true)}
      />

      <Cart
        cartOpen={cartOpen}
        onCartClose={() => setCartOpen(false)}
        cartItems={cartItems}
        addToCart={handleAddToCart}
        removeFromCart={handleRemoveFromCart}
        clearCart={handleClearCart}
      />

      <Container sx={{ marginTop: "80px", padding: "20px" }}>
        <Grid container spacing={3}>
          <ProductList
            products={paginatedData}
            onAction={handleAddToCart}
            actionLabel="Add to Cart"
          />
        </Grid>

        <Box
          sx={{ display: "flex", justifyContent: "center", marginTop: "20px" }}
        >
          <Pagination
            count={Math.ceil((data?.length || 0) / itemsPerPage)}
            page={page}
            onChange={(_, value) => setPage(value)}
            color="primary"
          />
        </Box>
      </Container>
    </Box>
  );
}
