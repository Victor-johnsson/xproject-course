import React, { useState } from "react";
import {
  Box,
  Typography,
  Paper,
  Button,
  Collapse,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  Drawer,
  InputAdornment,
} from "@mui/material";
import { CartItemType, performPayment, placeOrder } from "../Services/service";
import CartItem from "./CartItem";
import "../styles.css";

interface Props {
  cartItems: CartItemType[];
  addToCart: (clickedItem: CartItemType) => void;
  removeFromCart: (id: string) => void;
  clearCart: () => void;
  cartOpen: boolean;
  onCartClose: () => void;
}

export default function Cart({
  cartItems,
  addToCart,
  removeFromCart,
  clearCart,
  cartOpen,
  onCartClose,
}: Props) {
  const [isCheckoutPressed, setIsCheckoutPressed] = useState(false);
  const [isCustomerInfoVisible, setIsCustomerInfoVisible] = useState(false);
  const [isPaymentVisible, setIsPaymentVisible] = useState(false);
  const [orderConfirmed, setOrderConfirmed] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMessage, setDialogMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    address: "",
    cardName: "",
    cardNumber: "",
  });

  const calculateTotal = (items: CartItemType[]) =>
    items.reduce((acc, item) => acc + item.amount * item.price, 0).toFixed(2);

  const handleCheckout = () => {
    setIsCheckoutPressed(true);
    setIsCustomerInfoVisible(true);
  };

  const handleProceedToPayment = () => {
    setIsCustomerInfoVisible(false);
    setIsPaymentVisible(true);
  };

  const order = {
    orderLines: cartItems.map((item) => ({
      productId: item.id,
      itemCount: item.amount,
    })),
    customer: {
      name: formData.name,
      address: formData.address,
      email: formData.email,
    },
  };

  const handlePay = async () => {
    setLoading(true);
    try {
      const paymentResponse = await performPayment();
      const paymentId = paymentResponse;

      try {
        await placeOrder(order, paymentId.toString());
        clearCart();
        setOrderConfirmed(true);
        setDialogMessage("Order received! Thank you for your purchase.");
      } catch (orderError) {
        console.error("Order placement failed", orderError);
        setOrderConfirmed(false);
        setDialogMessage("Order placement failed. Please try again.");
      }
    } catch (paymentError) {
      console.error("Payment failed", paymentError);
      setOrderConfirmed(false);
      setDialogMessage("Payment failed. Please try again.");
    } finally {
      setLoading(false);
      setIsCustomerInfoVisible(false);
      setIsPaymentVisible(false);
      setDialogOpen(true);
    }
  };

  const handleDialogClose = () => {
    setDialogOpen(false);
    if (orderConfirmed) {
      onCartClose();
    }
  };

  const isCustomerInfoValid =
    formData.name.trim() !== "" &&
    formData.email.trim() !== "" &&
    formData.address.trim() !== "";

  const isPaymentInfoValid =
    formData.cardName.trim() !== "" && formData.cardNumber.trim().length === 19;

  return (
    <Drawer anchor="right" open={cartOpen} onClose={onCartClose}>
      <Paper elevation={3} sx={{ width: "500px", padding: "20px" }}>
        <Typography variant="h5" gutterBottom>
          Your Cart
        </Typography>

        {cartItems.length > 0 && (
          <Button
            onClick={() => setIsCheckoutPressed(!isCheckoutPressed)}
            variant="outlined"
            sx={{ marginBottom: "10px" }}
          >
            {isCheckoutPressed ? "Hide Checkout" : "Show Checkout"}
          </Button>
        )}

        <Collapse in={!isCheckoutPressed}>
          {cartItems.length === 0 ? (
            <Typography variant="body1">No items in cart.</Typography>
          ) : (
            cartItems.map((item) => (
              <CartItem
                key={item.id}
                item={item}
                addToCart={addToCart}
                removeFromCart={removeFromCart}
              />
            ))
          )}
          <Typography variant="h5" sx={{ marginTop: "20px" }}>
            Total: ${calculateTotal(cartItems)}
          </Typography>
        </Collapse>

        {!isCheckoutPressed && cartItems.length > 0 && (
          <Button
            fullWidth
            variant="contained"
            color="primary"
            onClick={handleCheckout}
            sx={{ marginTop: "20px" }}
          >
            Checkout
          </Button>
        )}

        <Collapse in={isCustomerInfoVisible}>
          <Box sx={{ marginTop: "20px" }}>
            <Typography variant="h6">Customer Information</Typography>
            <TextField
              fullWidth
              label="Full Name"
              variant="outlined"
              margin="normal"
              required
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
            />
            <TextField
              fullWidth
              label="Email Address"
              variant="outlined"
              margin="normal"
              required
              type="email"
              value={formData.email}
              onChange={(e) =>
                setFormData({ ...formData, email: e.target.value })
              }
            />
            <TextField
              fullWidth
              label="Address"
              variant="outlined"
              margin="normal"
              required
              value={formData.address}
              onChange={(e) =>
                setFormData({ ...formData, address: e.target.value })
              }
            />
            <Button
              fullWidth
              variant="contained"
              color="primary"
              sx={{ marginTop: "10px" }}
              onClick={handleProceedToPayment}
              disabled={!isCustomerInfoValid}
            >
              Proceed to Payment
            </Button>
          </Box>
        </Collapse>

        <Collapse in={isPaymentVisible}>
          <Box sx={{ marginTop: "20px" }}>
            <Typography variant="h6">Payment Information</Typography>
            <TextField
              fullWidth
              label="Card Name"
              variant="outlined"
              margin="normal"
              required
              value={formData.cardName}
              onChange={(e) =>
                setFormData({ ...formData, cardName: e.target.value })
              }
            />
            <TextField
              fullWidth
              label="Card Number"
              variant="outlined"
              margin="normal"
              required
              value={formData.cardNumber}
              onChange={(e) => {
                let value = e.target.value.replace(/\D/g, "");
                value = value.replace(/(.{4})/g, "$1 ").trim();
                if (value.length > 19) return;
                setFormData({ ...formData, cardNumber: value });
              }}
              inputProps={{ maxLength: 19 }}
              placeholder="1234 5678 9012 3456"
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">💳</InputAdornment>
                ),
              }}
            />
            <Button
              fullWidth
              variant="contained"
              color="success"
              sx={{ marginTop: "10px" }}
              onClick={handlePay}
              disabled={!isPaymentInfoValid || loading}
            >
              {loading ? "Processing..." : "Pay Now"}
            </Button>
          </Box>
        </Collapse>

        <Dialog open={dialogOpen} onClose={handleDialogClose}>
          <DialogTitle>
            {orderConfirmed ? "Order Confirmation" : "Payment Error"}
          </DialogTitle>
          <DialogContent>
            <Typography variant="h6" sx={{ textAlign: "center" }}>
              {dialogMessage}
            </Typography>
            <Button
              onClick={handleDialogClose}
              variant="contained"
              color="primary"
              sx={{ display: "block", margin: "20px auto" }}
            >
              {orderConfirmed ? "Close" : "Try Again"}
            </Button>
          </DialogContent>
        </Dialog>
      </Paper>
    </Drawer>
  );
}
