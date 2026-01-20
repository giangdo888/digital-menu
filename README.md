# Digital Menu with Personalised Nutrition Feedback

A web application that provides digital restaurant menu labelling, including nutrition labels for individual dishes, with personalised nutrition feedback based on users’ health profiles. This master’s project addresses a key gap in current nutrition applications, where frequent diners lack practical tools to understand and manage their dietary intake when eating out.

## Features

- **Multi-restaurant platform** - System admin manages restaurants, each with their own admin
- **Digital menu with nutrition info** - Dishes display calories, protein, carbs, fat
- **AFCD integration** - Australian Food Composition Database for accurate nutrition data
- **Personalised feedback** - Based on user's BMI, TDEE, and dietary goals
- **Meal logging** - Track daily intake with traffic light indicators
- **Weekly summaries** - Progress tracking with weight history

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Next.js 14, TailwindCSS, TypeScript |
| Backend | ASP.NET Core Web API, Entity Framework Core |
| Database | SQL Server (Docker) |
| Storage | AWS S3 (images) |
| Auth | JWT |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd digital-menu
```

### 2. Set up the database

Create a `.env` file in the root directory:

```bash
# Copy the example file
cp .env.example .env

# Edit .env and set your password
SA_PASSWORD=YourSecurePassword123
```

Start SQL Server container:

```bash
docker-compose up -d
```

### 3. Configure backend secrets

```bash
cd DigitalMenuApi

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=DigitalMenuDb;User Id=sa;Password=YOUR_PASSWORD_HERE;TrustServerCertificate=True"
```

### 4. Run database migrations

```bash
cd DigitalMenuApi
dotnet ef database update --connection "Server=localhost,1433;Database=DigitalMenuDb;User Id=sa;Password=YOUR_PASSWORD_HERE;TrustServerCertificate=True"
```

### 5. Run the backend

```bash
cd DigitalMenuApi
dotnet run
```

API will be available at: `http://localhost:5117`
Swagger UI: `http://localhost:5117/swagger`

### 6. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend will be available at: `http://localhost:3000`

## Project Structure

```
digital-menu/
├── DigitalMenuApi/           # ASP.NET Core Web API
│   ├── Controllers/          # API endpoints
│   ├── Data/                 # DbContext & configurations
│   ├── DTOs/                 # Request/Response objects
│   ├── Extensions/           # Service registration
│   ├── Models/Entities/      # EF Core entities
│   ├── Repositories/         # Data access layer
│   └── Services/             # Business logic
├── frontend/                 # Next.js frontend
│   └── src/
│       ├── app/              # App router pages
│       ├── components/       # React components
│       └── services/         # API calls
├── docker-compose.yml        # SQL Server container
└── .env                      # Environment variables (not committed)
```

## User Roles

| Role | Description |
|------|-------------|
| System Admin | Manages restaurants and system settings |
| Restaurant Admin | Manages their restaurant's menu and dishes |
| Customer | Browses menus, logs meals, views nutrition feedback |

## License

Private project - All rights reserved
