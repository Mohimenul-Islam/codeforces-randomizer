# Codeforces Randomizer

A full-stack web application for competitive programmers to find Codeforces problems unsolved by their practice group.

## Features

- 🎯 **Random Unsolved Problems** — Get problems nobody in your group has solved
- 👥 **Practice Groups** — Save groups of usernames (up to 10 groups, 20 users each)
- 🔐 **User Authentication** — Register/login with email, secure password reset via email
- 🔢 **Rating Filter** — Filter by difficulty range (800-3500)
- ⚡ **Real-time Validation** — Validates Codeforces handles against their API

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core 10, C# 13 |
| **Frontend** | Razor Pages |
| **Database** | PostgreSQL + EF Core |
| **Auth** | Cookie & JWT authentication |
| **Email** | MailKit (SMTP) |
| **Containerization** | Docker + Docker Compose |

## Quick Start

### With Docker (Recommended)

```bash
git clone https://github.com/Mohimenul-Islam/codeforces-randomizer.git
cd codeforces-randomizer
docker-compose up --build
# Open http://localhost:8080
```

### Without Docker

```bash
# Prerequisites: .NET 10 SDK, PostgreSQL

# Configure secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=codeforces_randomizer;Username=postgres;Password=yourpassword"
dotnet user-secrets set "Jwt:SecretKey" "your-32-char-secret-key"

# Run
dotnet run
# Open http://localhost:5096
```

## API Endpoints

### Authentication
- `POST /api/auth/register` — Create account
- `POST /api/auth/login` — Get JWT token
- `GET /api/auth/me` — Get current user

### Practice Groups
- `GET /api/groups` — List your groups
- `POST /api/groups` — Create group
- `PUT /api/groups/{id}` — Update group
- `DELETE /api/groups/{id}` — Delete group

### Problems
- `POST /api/problems/random` — Get random unsolved problems

## Project Structure

```
CodeforcesRandomizer/
├── Controllers/          # API endpoints
├── Pages/                # Razor Pages (web UI)
│   ├── Auth/             # Login, Register, Password Reset
│   ├── Groups/           # CRUD for practice groups
│   └── Search.cshtml     # Problem search interface
├── Services/             # Business logic
├── Models/               # Entities and DTOs
├── Migrations/           # EF Core migrations
├── Dockerfile            # Production container
└── docker-compose.yml    # Local dev environment
```

## Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__SecretKey` | JWT signing key (32+ chars) |
| `Email__Username` | SMTP email address |
| `Email__Password` | SMTP password/app password |

## License

MIT
