# Codeforces Randomizer API

A REST API that finds Codeforces problems unsolved by you and your friends — perfect for group practice sessions.

## Features

- 🎯 **Random Unsolved Problems** — Get problems you haven't solved yet
- 👥 **Group Practice** — Find problems nobody in your group (up to 20 people) has solved
- 🔢 **Rating Filter** — Filter by difficulty range (800-3500)
- ⚡ **Parallel Fetching** — Fast response with concurrent API calls
- 🔍 **Smart Validation** — Reports all invalid usernames in one response

## Tech Stack

- **ASP.NET Core 10** — Minimal API with Controllers
- **C# 13** — Modern language features
- **Swagger/OpenAPI** — Interactive API documentation

## Quick Start

```bash
# Clone and run
git clone https://github.com/Mohimenul-Islam/codeforces-randomizer.git
cd codeforces-randomizer
dotnet run

# Open Swagger UI
# http://localhost:5096/swagger
```

## API Usage

### Get Random Unsolved Problems

```http
POST /api/problems/random
Content-Type: application/json

{
  "usernames": ["tourist", "Petr"],
  "count": 5,
  "minRating": 1500,
  "maxRating": 2000
}
```

### Parameters

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `usernames` | string[] | required | 1-20 Codeforces handles |
| `count` | int | 5 | Number of problems to return |
| `minRating` | int | 800 | Minimum problem rating |
| `maxRating` | int | 2000 | Maximum problem rating |

### Example Response

```json
[
  {
    "problemId": "1130C",
    "name": "Connect",
    "rating": 1400,
    "tags": ["brute force", "dfs and similar", "dsu"],
    "url": "https://codeforces.com/problemset/problem/1130/C"
  }
]
```

### Error Handling

| Status | Description |
|--------|-------------|
| `400` | Invalid request (0 users, >20 users) |
| `404` | User(s) not found — lists all invalid usernames |
| `502` | Codeforces API unavailable |

## Project Structure

```
CodeforcesRandomizer/
├── Controllers/
│   └── ProblemsController.cs    # API endpoint
├── Services/
│   ├── ICodeforcesService.cs    # Service interface
│   └── CodeforcesService.cs     # Codeforces API integration
├── Models/
│   ├── ProblemDto.cs            # Response model
│   └── Codeforces/              # Codeforces API models
├── Exceptions/
│   ├── UserNotFoundException.cs
│   └── CodeforcesApiException.cs
└── Program.cs                    # App configuration
```

## License

MIT
