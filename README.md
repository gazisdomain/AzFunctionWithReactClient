# Azure Function App with React Client

![Build](https://github.com/gazisdomain/AzFunctionWithReactClient/actions/workflows/build.yml/badge.svg)


This project demonstrates a **full-stack application** using:

- **Azure Functions (Isolated .NET 8)** – backend API
- **Azure Cosmos DB (via SDK)** – data persistence
- **React (Vite)** – frontend client

It shows how to build a serverless backend with .NET and connect it to a modern JavaScript frontend.

---

## 📂 Project Structure

```
AzFunctionWithReactClient/
├─ TodoFunctions/            # Azure Function App (C# / .NET 8)
│  ├─ Program.cs
│  ├─ TodoFunctions.csproj
│  └─ Functions/...
├─ AzFuncAppClientReact/     # React client (Vite + JS)
│  ├─ src/App.jsx
│  ├─ package.json
│  └─ .env (for API base URL)
└─ README.md
```

---

## 🚀 Features
- **Function App (API)**
  - CRUD endpoints for a Todo list
  - Connected to Cosmos DB
  - OpenAPI (Swagger) documentation enabled

- **React Client**
  - Calls the Azure Functions API
  - Lists todos, adds new todos
  - Modern React stack (hooks, JSX, Vite)

---

## 🛠️ Tech Stack
- **Backend**: Azure Functions, .NET 8 (Isolated Worker)
- **Database**: Azure Cosmos DB (local emulator or cloud)
- **Frontend**: React (Vite, JSX)
- **Language**: C# (server) + JavaScript (client)

---

## 🔧 Running Locally

### 1. Start Azure Functions
```bash
cd TodoFunctions
# Open in Visual Studio or run with Core Tools:
func start
```
- Runs on: `http://localhost:7050`
- Swagger UI: `http://localhost:7050/api/swagger/ui`

### 2. Start React Client
```bash
cd AzFuncAppClientReact
npm install
npm run dev
```
- Runs on: `http://localhost:5173`

The React app calls the Function API at the URL set in `.env`.

---

## ⚙️ Configuration

### Function App
Create `TodoFunctions/local.settings.json` (not committed):
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Cosmos__EndpointUri": "https://localhost:8081",
    "Cosmos__PrimaryKey": "<your-cosmos-key>",
    "Cosmos__Database": "TodoDb",
    "Cosmos__Container": "Todos"
  },
  "Host": {
    "CORS": "http://localhost:5173"
  }
}
```

### React Client
Create `AzFuncAppClientReact/.env`:
```
VITE_API_BASE=http://localhost:7050/api
```

---

## 📸 Screenshots (optional)
*(Add screenshots here of Swagger UI and React client UI for extra impact.)*

---

## 🎯 Why this project?
This project demonstrates:
- Building serverless APIs with Azure Functions
- Using dependency injection in .NET isolated worker
- Connecting to Cosmos DB
- Documenting APIs with OpenAPI (Swagger)
- Creating a React frontend that consumes the API
- Managing local dev with multiple apps (Functions + React)

---

## 📌 Next Steps (Future Work)
- Deploy Function App to Azure
- Deploy React client to Azure Static Web Apps
- Add authentication (Azure AD B2C)
- Extend with more features (mark todo done, delete todo, etc.)
