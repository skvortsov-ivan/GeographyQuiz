# GeographyQuiz API  
A ASP.NET Core 9 Web API where players compete in a population‑based geography game.  
The API fetches real‑world country data, runs a 5‑round elimination game, and stores results in an in‑memory leaderboard.  
It includes JWT authentication, rate limiting, hybrid caching, Swagger documentation, and global exception handling.

## Table of Contents
- Overviewoverview
- Featuresfeatures
- Tech Stacktech-stack
- Architecturearchitecture
- User Secrets Setup
- Installation
- Performance Measurement

## Overview
GeographyQuiz is a backend‑only game where the player is shown two countries and must guess which one has the larger population.  
The game lasts **5 rounds**.

The API uses:

- **API Ninjas Country API** for real population data  
- **HybridCache** to reduce external API calls  
- **JWT tokens** for admin‑only leaderboard management  
- **Rate limiting** to protect endpoints  
- **ProblemDetails** for consistent error responses  

## Features

### Game System
- 5‑round elimination game  
- Random country selection  
- Winner carries over to next round  
- Prevents skipping rounds or double‑answering  
- Full reset functionality  

### Leaderboard
- Add scores  
- Filter by name or date  
- Paginated results  
- Admin‑only update/delete  

### Authentication
- JWT‑based login  
- Admin role baked into token  
- Required for modifying leaderboard entries  

### Rate Limiting
- Sliding window for GET endpoints  
- Fixed window for POST endpoints  
- Admin endpoints bypass limits  

### Developer Experience
- Swagger UI with JWT support  
- Global exception handling  
- Typed HttpClient  
- Clean DTO‑based responses  

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| **ASP.NET Core 9** | Web API framework |
| **HybridCache** | Caching external API responses |
| **HttpClient (Typed)** | API Ninjas integration |
| **JWT Authentication** | Admin login & role-based access |
| **Rate Limiting Middleware** | Request throttling |
| **Swagger / OpenAPI** | API documentation |
| **ProblemDetails (RFC 7807)** | Standardized error responses |

## User Secrets Setup

This project requires a few secrets to run locally.  
Make sure you are inside the project folder before running these commands.

### API Ninjas Key  
Used for fetching real population data.

### JWT Secret Key  
Used for signing authentication tokens.

## Installation

```bash
git clone https://github.com/<skvortsov-ivan>/GeographyQuiz.git
cd GeographyQuiz
dotnet restore
dotnet run
dotnet user-secrets set "ApiNinjas:ApiKey" "<YOUR_API_KEY>"
dotnet user-secrets set "Jwt:Key" "<YOUR_SECRET_KEY>"
```

## Performance Measurement (Cache Hit vs Cache Miss)

The API uses HybridCache to reduce external calls to the API Ninjas service.  
To demonstrate the performance difference, the `/api/games/round` endpoint was measured under two conditions.

### Cache Miss  
This is the first time a country is requested, and the API must call the external service.

Typical response time: 400-800 ms

This includes network latency, external API processing, and JSON parsing.

### Cache Hit  
This occurs when the same country is requested again and the data is already stored in the cache.

Typical response time is less than 1 ms

### Summary  
A cached request is in the order of 100 times faster than a non‑cached request.  
This improves performance during gameplay, since countries can repeat across rounds.


