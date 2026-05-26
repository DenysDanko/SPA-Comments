
# SPA Comments

Web application for creating and managing comments with file uploads and real-time updates.

## 🛠 Technology Stack

### Backend
- .NET 10 (Preview)
- Entity Framework Core
- SignalR
- FluentValidation
- IMemoryCache
- SQL Server

### Frontend
- React
- Axios
- SignalR Client
- Bootstrap 5

### Other
- Docker
- Docker Compose

---

## 📥 Clone Repositories

Clone backend: git clone https://github.com/DenysDanko/SPA-Comments.git

Clone frontend: git clone https://github.com/DenysDanko/comment-ui.git

---

## ▶️ Run Backend

Before running backend, make sure you have Microsoft SQL Server (MSSQL) installed.

Open `appsettings.json` and configure connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=<YourServerName>;Database=CommentsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Example for LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Database=CommentsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Go to backend folder:

cd SPA-Comments
dotnet run

## ▶️ Run Frontend

Go to frontend folder:

cd comment-ui
npm install
npm start

Frontend runs on: http://localhost:3000