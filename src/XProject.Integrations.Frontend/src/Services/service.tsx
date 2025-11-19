export type CartItemType = {
  id: string;
  name: string;
  price: number;
  stock: number;
  imageUrl: string;
  amount: number;
};
// Get all products
export const getProducts = async (): Promise<CartItemType[]> => {
  await new Promise((resolve) => setTimeout(resolve, 1000));

  const response = await fetch(`/api/products`, {
    method: "GET",
  });

  if (!response.ok) {
    console.error("Fetch failed:", response.status, response.statusText);
    throw new Error(`HTTP error! Status: ${response.status}`);
  }

  return response.json();
};

// Delete a product by id
export const deleteProduct = async (
  id: string,
  token: string,
): Promise<void> => {
  const response = await fetch(`/api/products/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    console.error("Delete failed:", response.status, response.statusText);
    throw new Error(`Failed to delete product with ID: ${id}`);
  }
};

export type ProductInputType = {
  name: string;
  price: number;
  stock: number;
  image: File | null;
};

export type ProductResponseType = {
  id: string;
  name: string;
  price: number;
  stock: number;
  imageUrl: string;
};

// Convert Image to Base64
const convertImageToBase64 = async (image: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(image);
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = (error) => reject(error);
  });
};

// Add a products
export const addProduct = async (
  product: ProductInputType,
  token: string,
): Promise<void> => {
  try {
    let imageBase64 = "";

    // Convert image to Base64
    if (product.image) {
      imageBase64 = await convertImageToBase64(product.image);
    }

    const response = await fetch(`/api/products`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        name: product.name,
        price: product.price,
        stock: product.stock,
        imageBase64: imageBase64,
      }),
    });

    if (!response.ok) {
      console.error(
        "Add product failed:",
        response.status,
        response.statusText,
      );
      throw new Error("Failed to add product.");
    }
  } catch (error) {
    console.error("Error in addProduct:", error);
    throw error;
  }
};

export const validateToken = async (token: string): Promise<boolean> => {
  try {
    const response = await fetch(`/api/Auth/login`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error(`Token validation failed. Status: ${response.status}`);
    }

    return true;
  } catch (error) {
    console.error("Error validating token:", error);
    return false;
  }
};

export type PaymentResponseType = {
  Id: string;
  PaymentCompleted: string;
};

export const performPayment = async (): Promise<PaymentResponseType> => {
  const response = await fetch(`/api/payments`, {
    method: "POST",
  });

  if (!response.ok) {
    console.error("Fetch failed:", response.status, response.statusText);
    throw new Error(`HTTP error! Status: ${response.status}`);
  }

  return response.json();
};

export type OrderLine = {
  productId: string;
  itemCount: number;
};

export type Customer = {
  name: string;
  address: string;
  email: string;
};

export type Order = {
  orderLines: OrderLine[];
  customer: Customer;
};

export const placeOrder = async (
  order: Order,
  paymentId: string,
): Promise<void> => {
  try {
    const response = await fetch(`/api/orders`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        orderLines: order.orderLines,
        customer: order.customer,
        paymentId: paymentId,
      }),
    });

    if (!response.ok) {
      console.error(
        "Failed placing an order:",
        response.status,
        response.statusText,
      );
      throw new Error("Failed to place the order.");
    }
  } catch (error) {
    console.error("Error in placeOrder:", error);
    throw error;
  }
};
