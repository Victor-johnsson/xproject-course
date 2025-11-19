# Exercise 01 – Getting Started with XProject (Local Aspire Setup)

## Objective

In this exercise, you’ll set up the **XProject eShop environment** locally using **.NET Aspire**.
The goal is to start all services, verify that the system runs end-to-end, and understand the project structure.

---

## 1. Overview

Welcome to **XProject**!

This repository contains the initial setup for your integration exercises.
You’ll work with an eShop-like scenario consisting of multiple connected services.

### Provided projects:

| Project | Description |
|----------|--------------|
| **AppHost** | Main entrypoint; orchestrates all projects using .NET Aspire. |
| **Backend** | Core logic and APIs you’ll extend during later exercises. |
| **FunctionApp** | Azure Function project — will host integration triggers later. |
| **Frontend** | Example frontend (acts as an external consumer of the backend). |
| **CRM**, **PIM**, **PSP** | Simulated external systems used for integration exercises. |
| **ServiceDefaults** | Centralized configuration shared between all services. |

> **Note:** `Aspire` is **not** a replacement for Azure — it’s an excellent way to run and orchestrate cloud-style applications locally.

---

## 2. Prerequisites

Before starting, ensure you have the following tools installed:

| Tool | Minimum Version | Notes |
|------|------------------|-------|
| Visual Studio | 2022 or newer | (with .NET support) |
| **.NET SDK** | 10.0.x | required for building and running |
| Docker Desktop or Podman| Latest | required for local databases |
| Git | Latest | to clone the repository |
| Azure Functions Core Tools | Latest | for running FunctionApp locally |
| **Optional:** Aspire CLI | see installation below | required to run `aspire run` manually |
| **Optional:** Aspire Extension | see installation below | VsCode Extension |

### Optional: Install Aspire CLI

**Windows (PowerShell):**
```powershell
Invoke-Expression "& { $(Invoke-RestMethod https://aspire.dev/install.ps1) }"
```

**Mac/Linux (bash):**
```bash
curl -sSL https://aspire.dev/install.sh | bash
```

---

## 3. Understanding the Structure

The **PIM**, **CRM**, **PSP**, and **Frontend** projects are **fully functional**.
They represent external systems that your backend will later integrate with.

You will implement logic primarily in:
- **Backend**
- **FunctionApp**

The **AppHost** project glues everything together via **Aspire**, handles:
- Local orchestration of multiple projects
- Connection string injection (e.g., PostgreSQL, Cosmos DB)
- Development-time secret handling

---

## 4. Running the Solution

There are two ways to start your environment:

### Option 1 – Run from Visual Studio / VS Code
Simply **build and start the solution** (`F5` or `Run`).
Visual Studio will automatically open the Aspire Dashboard.

### Option 2 – Run using Aspire CLI
Use this if you prefer command line execution.

```bash
# From the root folder
aspire run
```

This will:
- Build all projects
- Start required containers (PostgreSQL, Cosmos DB, etc.)
- Spin up all microservices locally

You’ll get a URL to the **Aspire Dashboard** (or see it automatically in VS).

---

## 5. Verifying Your Environment

When everything starts successfully, you should:

- See all projects listed as “Healthy” in the **Aspire Dashboard**
- Confirm databases (PSP, CRM, PIM) are running in Docker containers
- Access individual service URLs (e.g. Backend API, CRM API) from the dashboard

> None of the databases are pre-loaded.
> You can use CRUD API endpoints for each project to seed test data.
> Data persists between container restarts unless you remove Docker volumes.

---

## 6. Troubleshooting

- **PIM not starting on first run:**
  Occasionally, the PIM project fails to connect to its CosmosDB container on initial startup.
  **Fix:** Start the project once, wait for dependencies to initialize, then stop and restart the entire solution. It should then work fine on subsequent runs.

---

## 7. Summary

✅ You’ve now:
- Installed all required dependencies
- Started the complete system using Aspire
- Verified all services are running locally

You’re ready to begin **Exercise 02 – Implementing Azure Integration**!

---

**Next:** [Exercise 02 – Integrating with ProductService »](../02-http-clients/2_1_ImplementingProductService.md)
