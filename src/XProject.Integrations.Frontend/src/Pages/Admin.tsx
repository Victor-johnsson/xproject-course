import React, { useState } from "react";
import {
  Box,
  Container,
  TextField,
  LinearProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  CircularProgress,
} from "@mui/material";
import { useQuery, useQueryClient } from "react-query";
import {
  CartItemType,
  getProducts,
  deleteProduct,
  ProductInputType,
  addProduct,
} from "../Services/service";
import AppBarComponent from "../Components/AppBarComponent";
import ProductList from "../Components/ProductList";
import AddProductForm from "../Components/AddProductForm";
import AuthContext from "../Authentication/AuthContext";

export default function AdminWebshop({ token }: { token: string }) {
  const queryClient = useQueryClient();
  const { data, isLoading, error } = useQuery<CartItemType[]>(
    ["products"],
    getProducts,
  );
  const [searchTerm, setSearchTerm] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [productToDelete, setProductToDelete] = useState<CartItemType | null>(
    null,
  );
  const { handleLogout } = React.useContext(AuthContext);

  if (isLoading) return <LinearProgress />;
  if (error) return <div>Something went wrong</div>;

  const filteredData =
    data?.filter(
      (item) =>
        item.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.id.includes(searchTerm),
    ) || [];

  const openConfirmDialog = (clickedItem: CartItemType) => {
    setProductToDelete(clickedItem);
  };

  const handleConfirmDelete = async () => {
    if (productToDelete) {
      try {
        setActionLoading(true);
        await deleteProduct(productToDelete.id, token);
        // console.log("Product deleted:", productToDelete);
        // Wait 5 seconds, then refetch products
        await new Promise((resolve) => setTimeout(resolve, 5000));
        await queryClient.refetchQueries(["products"]);
      } catch (error) {
        console.error("Error deleting product:", error);
      } finally {
        setActionLoading(false);
        setProductToDelete(null);
      }
    }
  };

  // If user cancels deletion, close dialog
  const handleCancelDelete = () => {
    setProductToDelete(null);
  };

  // Handle adding product and refetch after 5 seconds
  const handleAddProduct = async (product: ProductInputType) => {
    try {
      setActionLoading(true);
      await addProduct(product, token);
      // console.log("New Product Added:", product);
      await new Promise((resolve) => setTimeout(resolve, 5000));
      await queryClient.refetchQueries(["products"]);
    } catch (error) {
      console.error("Error adding product:", error);
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <Box>
      <AppBarComponent
        title="WebshopX Admin"
        logoutButton
        onLogoutClick={() => handleLogout()}
      />

      <Container sx={{ marginTop: "80px" }}>
        <TextField
          fullWidth
          label="Search Products"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          sx={{ marginBottom: "20px" }}
        />

        <ProductList
          products={filteredData}
          onAction={openConfirmDialog}
          actionLabel="Delete Product"
          isAdmin={true}
          onAddProduct={() => setFormOpen(true)}
        />
      </Container>

      {/* Add Product Form */}
      <AddProductForm
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={handleAddProduct}
      />

      {/* Confirmation Dialog for Deletion */}
      <Dialog open={Boolean(productToDelete)} onClose={handleCancelDelete}>
        <DialogTitle>Confirm Deletion</DialogTitle>
        <DialogContent>
          Are you sure you want to delete this product?
        </DialogContent>
        <DialogActions>
          <Button
            variant="contained"
            onClick={handleCancelDelete}
            color="primary"
          >
            No
          </Button>
          <Button
            variant="contained"
            onClick={handleConfirmDelete}
            color="error"
            autoFocus
          >
            Yes
          </Button>
        </DialogActions>
      </Dialog>

      {/* Circular Progress Overlay */}
      {actionLoading && (
        <Box
          sx={{
            position: "fixed",
            top: 0,
            left: 0,
            width: "100vw",
            height: "100vh",
            backgroundColor: "rgba(0,0,0,0.3)",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            zIndex: 9999,
          }}
        >
          <CircularProgress />
        </Box>
      )}
    </Box>
  );
}
