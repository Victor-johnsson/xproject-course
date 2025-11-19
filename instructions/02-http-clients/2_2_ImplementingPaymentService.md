
# Exercise 03 – Implementing Payment Service Integration

## Objective

In this exercise, you’ll implement an integration with the **Payment Service (PSP)**.
This integration will follow the same structure as the Product (PIM) integration, but you’ll now design and implement it **on your own**.

---

## Background

The **PSP** (Payment Service Provider) acts as another external system in the eShop environment.
You will integrate your backend with this system to support payment operations such as initiating and validating transactions.

Just as in the previous exercise, the PSP service is already hosted in the Aspire environment and available internally through:

```
http://paymentService
```

Your backend will communicate with this service using a typed `HttpClient` and an interface abstraction.

---

## Task Overview

1. **Implement the service** using `HttpClient` — similar to how you implemented `PimService`.
   - Register the client with `builder.Services.AddHttpClient<IPaymentService, PaymentService>()`.
   - Use the Aspire service URL (`http://paymentService`) as the base address.

2. **Expose your integration** through a REST API endpoint.
   - You can use either **Minimal API** or **Controllers** — your choice.

---

## Hints & Guidance

- Look back at your **Product Service (PIM)** implementation for inspiration — the same integration principles apply.
- Handle failed HTTP responses cleanly, log relevant information, and ensure you don’t leak sensitive data.
- For local testing, use the PSP service endpoints visible in the Aspire dashboard — they simulate real payment gateway behavior.

---

## Checkpoints

✅ Your backend builds and runs without errors.
✅ You can successfully call the endpoints of your PSP integration from tools like Postman or the Aspire dashboard.
✅ The pattern you used is consistent with how you built the `PimService`.


---

## Summary

This exercise tests your ability to **transfer knowledge** and independently apply integration patterns.
By completing it, you’ve established two external system links: product and payment services — just like you would in a real multi‑service eCommerce backend.

---

**Next:** [Exercise 03 – Integrate to blob storage »](../03-blobstorage/3_1_integrate-blob-storage.md)
