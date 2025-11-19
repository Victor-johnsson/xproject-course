import React, { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogTitle,
  TextField,
  Button,
  Box,
  Typography,
  IconButton,
  Paper,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import "../styles.css";

const MAX_FILE_SIZE = 5 * 1024 * 1024;

interface Props {
  open: boolean;
  onClose: () => void;
  onSubmit: (product: {
    name: string;
    price: number;
    stock: number;
    image: File | null;
  }) => void;
}

export default function AddProductForm({ open, onClose, onSubmit }: Props) {
  const [productName, setProductName] = useState("");
  const [productPrice, setProductPrice] = useState<number | "">("");
  const [stock, setStock] = useState<number | "">("");
  const [image, setImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);

  const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      if (file.size > MAX_FILE_SIZE) {
        alert("File is too large. Please select an image smaller than 5 MB.");
        setImage(null);
        setImagePreview(null);
        return;
      }
      setImage(file);
      setImagePreview(URL.createObjectURL(file));
    }
  };

  const handleSubmit = () => {
    if (!productName || !productPrice || !stock || !image) {
      alert("Please fill in all fields!");
      return;
    }

    onSubmit({
      name: productName,
      price: Number(productPrice),
      stock: Number(stock),
      image,
    });

    setProductName("");
    setProductPrice("");
    setStock("");
    setImage(null);
    setImagePreview(null);
    onClose();
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: {
          borderRadius: "20px",
          padding: "20px",
          width: "600px",
        },
      }}
    >
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        Add New Product
        <IconButton onClick={onClose}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <TextField
          fullWidth
          label="Product Name"
          variant="outlined"
          margin="dense"
          value={productName}
          onChange={(e) => setProductName(e.target.value)}
        />

        <TextField
          fullWidth
          label="Product Price"
          variant="outlined"
          margin="dense"
          type="number"
          value={productPrice}
          onChange={(e) => setProductPrice(Number(e.target.value))}
        />

        <TextField
          fullWidth
          label="Stock"
          variant="outlined"
          margin="dense"
          type="number"
          value={stock}
          onChange={(e) => setStock(Number(e.target.value))}
        />

        <Paper
          variant="outlined"
          sx={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            padding: "20px",
            marginTop: "10px",
            cursor: "pointer",
            border: "2px dashed #1976D2",
            borderRadius: "10px",
          }}
        >
          <input
            type="file"
            accept="image/*"
            onChange={handleImageUpload}
            style={{ display: "none" }}
            id="image-upload"
          />
          <label htmlFor="image-upload">
            <CloudUploadIcon fontSize="large" sx={{ color: "#1976D2" }} />
            <Typography
              variant="body1"
              sx={{ color: "#1976D2", cursor: "pointer" }}
            >
              {image ? "Change Image" : "Click to Upload"}
            </Typography>
          </label>
          {imagePreview && (
            <Box mt={2} display="flex" justifyContent="center">
              <img
                src={imagePreview}
                alt="Preview"
                style={{ maxWidth: "100%", maxHeight: "100px" }}
              />
            </Box>
          )}
        </Paper>

        <Button
          fullWidth
          variant="contained"
          color="primary"
          sx={{ marginTop: "20px", borderRadius: "10px" }}
          onClick={handleSubmit}
        >
          Add Product
        </Button>
      </DialogContent>
    </Dialog>
  );
}
