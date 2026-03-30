# SE4458 Midterm Project - Short-Term Stay Platform API

**Student:** Begum Bal
**Group:** 2
**Course:** SE 4458 - Software Architecture & Design of Modern Large Scale Systems
**Semester:** Spring 2025-26

## Project Overview

This project implements a backend API for a fictitious short-term stay company (similar to Airbnb). The system supports three user roles:

- **Host**: Manages property listings via mobile app
- **Guest**: Searches and books stays via mobile app
- **Admin**: Manages the platform via web admin panel

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Backend | .NET 8 Web API (C#) |
| Database | Azure Database for PostgreSQL Flexible Server |
| Authentication | JWT Bearer Token |
| API Gateway | Ocelot (with Rate Limiting) |
| API Docs | Swagger / OpenAPI |
| Containerization | Docker + Docker Compose |
| Cloud Hosting | Azure Container Apps + Azure Container Registry |
| Load Testing | k6 |

## Architecture

The project follows **service-oriented architecture** principles as discussed in class:

```
┌──────────────┐     ┌──────────────────┐     ┌──────────────┐
│  Mobile App  │────>│   API Gateway    │────>│   StayAPI    │
│  (Host/Guest)│     │  (Ocelot)        │     │  (.NET 8)    │
└──────────────┘     │  - Rate Limiting  │     │              │
                     │  - Routing        │     │  Controllers │
┌──────────────┐     │                  │     │      │       │
│  Web Admin   │────>│                  │     │  Services    │
│  Panel       │     └──────────────────┘     │      │       │
└──────────────┘                              │  Data Access │
                                              │      │       │
                                              │  PostgreSQL  │
                                              └──────────────┘
```

### Layered Architecture (No DB in Controllers)

```
Controller Layer  →  Receives HTTP requests, validates input, returns responses
       │                (Uses DTOs for request/response)
       ▼
Service Layer     →  Business logic, validation rules
       │                (Works with Domain Models)
       ▼
Data Access Layer →  Entity Framework Core DbContext
       │                (Manages database operations)
       ▼
PostgreSQL DB     →  Persistent storage
```

## Data Model (ER Diagram)

```
┌──────────────────┐
│      User        │
├──────────────────┤
│ Id (PK)          │
│ Username (UQ)    │
│ PasswordHash     │
│ Role             │
│ CreatedAt        │
└────────┬─────────┘
         │
         │ 1:N (as Host)
         ▼
┌──────────────────┐       ┌──────────────────┐
│     Listing      │       │     Booking      │
├──────────────────┤       ├──────────────────┤
│ Id (PK)          │──1:N──│ Id (PK)          │
│ HostUserId (FK)  │       │ ListingId (FK)   │
│ Title            │       │ GuestUserId (FK) │
│ Description      │       │ DateFrom         │
│ NoOfPeople       │       │ DateTo           │
│ Country          │       │ NamesOfPeople    │
│ City             │       │ Status           │
│ Price            │       │ CreatedAt        │
│ CreatedAt        │       └────────┬─────────┘
│ IsActive         │                │
└────────┬─────────┘                │ 1:1
         │                          ▼
         │ 1:N            ┌──────────────────┐
         └───────────────>│     Review       │
                          ├──────────────────┤
                          │ Id (PK)          │
                          │ BookingId (FK,UQ)│
                          │ ListingId (FK)   │
                          │ GuestUserId (FK) │
                          │ Rating (1-5)     │
                          │ Comment          │
                          │ CreatedAt        │
                          └──────────────────┘
```

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|---------|-------------|
| POST | `/api/v1/auth/register` | Register a new user |
| POST | `/api/v1/auth/login` | Login and get JWT token |

### Mobile App - Host
| Method | Endpoint | Auth | Paging | Description |
|--------|---------|------|--------|-------------|
| POST | `/api/v1/listings` | YES | NO | Insert a new listing |

### Mobile App - Guest
| Method | Endpoint | Auth | Paging | Description |
|--------|---------|------|--------|-------------|
| GET | `/api/v1/listings/search` | NO | YES (size 10) | Query available listings (rate limited: 3/day) |
| POST | `/api/v1/bookings` | YES | NO | Book a stay |
| POST | `/api/v1/reviews` | YES | NO | Review a completed stay |

### Web Site Admin
| Method | Endpoint | Auth | Paging | Description |
|--------|---------|------|--------|-------------|
| GET | `/api/v1/admin/report/listings` | YES | YES (size 10) | Report listings with ratings (supports `Country`, `City`, `MinRating`, `MaxRating`) |
| POST | `/api/v1/admin/listings/upload` | YES | NO | Insert listings by CSV file |

## API Gateway & Rate Limiting

The API Gateway is built with **Ocelot** and provides:
- Request routing to downstream StayAPI service
- **Rate limiting** on Query Listings endpoint (3 calls per day per client)
- Centralized entry point for all API calls

Deployment note:
The current [ocelot.json](/Users/begum/Documents/Claude/Projects/Large%20Scale%20Systems/SE4458-Midterm/ApiGateway/ocelot.json) uses `localhost:5001` as the downstream StayAPI host for local development. Before cloud deployment, these downstream host values must be changed to the deployed StayAPI domain and scheme.

## Assumptions

1. A listing can only be booked by one guest at a time for a given date range (no double booking).
2. Only guests who have completed a booking can leave a review (enforced by checking booking ownership).
3. Each booking can only be reviewed once.
4. The "Date" field in Query Listings filters listings that are NOT booked during the requested date range.
5. CSV file upload for listings expects columns: `NoOfPeople, Country, City, Price, Title`.
6. Rate limiting (3 calls/day) is implemented at the API Gateway level using Ocelot.
7. Listing capacity (NoOfPeople) represents the maximum number of guests the property can accommodate.
8. Admin role has access to all endpoints.
9. `Book a Stay` requests identify the target stay by `listingId`, even though the assignment description does not explicitly name this parameter.
10. `Review a Stay` uses `bookingId` as the stay identifier so that only completed bookings can be reviewed and each booking can only be reviewed once.

## How to Run Locally

### Prerequisites
- .NET 8 SDK
- PostgreSQL (or Docker)
- k6 (for load testing)

### Option 1: Docker Compose (Recommended)
```bash
docker-compose up --build
```
- API Gateway: http://localhost:5000
- StayAPI + Swagger: http://localhost:5001/api-docs
- PostgreSQL: localhost:5432

### Option 2: Manual
```bash
# 1. Start PostgreSQL and create database
createdb stayapi_dev

# 2. Run StayAPI
cd StayAPI
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run --urls http://localhost:5001

# 3. Run API Gateway (in another terminal)
cd ApiGateway
dotnet run --urls http://localhost:5000
```

### Swagger UI
Navigate to: `http://localhost:5001/api-docs/`

## Deployed URLs

| Component | URL |
|-----------|-----|
| StayAPI (Azure) | `https://stayapi-app.mangowater-b28dd996.swedencentral.azurecontainerapps.io` |
| StayAPI Swagger JSON | `https://stayapi-app.mangowater-b28dd996.swedencentral.azurecontainerapps.io/swagger/v1/swagger.json` |
| API Gateway (Azure) | `https://gateway-app.mangowater-b28dd996.swedencentral.azurecontainerapps.io` |
| Gateway Swagger JSON | `https://gateway-app.mangowater-b28dd996.swedencentral.azurecontainerapps.io/swagger/v1/swagger.json` |
| GitHub Repo | `https://github.com/bbgmfun/lodgestack-api` |
| Video Presentation | `<your-video-link>` |

## Load Testing

### Tools Used
- **k6** (https://k6.io/)

### Endpoints Tested
1. **Query Listings** (GET `/api/v1/listings/search`) - Public endpoint, no auth required
2. **Book a Stay** (POST `/api/v1/bookings`) - Authenticated endpoint

### Test Scenarios
| Scenario | Virtual Users | Duration |
|----------|--------------|----------|
| Normal Load | 20 VUs | 30 seconds |
| Peak Load | 50 VUs | 30 seconds |
| Stress Load | 100 VUs | 30 seconds |

### Running Load Tests
```bash
# Install k6: https://k6.io/docs/getting-started/installation/
k6 run k6/load-test.js
```

### Load Test Results

The following values are from a `k6` run on March 30, 2026 using the included multi-stage scenario set (20, 50, and 100 VUs).

| Metric | Result |
|--------|--------|
| Avg Response Time | 4.66 ms |
| Median Response Time | 2.31 ms |
| P90 Response Time | 11.28 ms |
| P95 Response Time | 15.27 ms |
| Max Response Time | 122.32 ms |
| Requests/sec | 101.41 req/s |
| Total Requests | 10,214 |
| Error Rate | 49.95% |
| Total Iterations | 5,100 |
| Max Virtual Users | 150 |

### Analysis

- The API met the latency target comfortably: `p95 = 15.27 ms`, which is well below the configured threshold of `2000 ms`.
- Throughput was stable at about `101.41 req/s` during the combined normal, peak, and stress scenarios.
- The failing threshold was `http_req_failed` with an error rate of `49.95%`. This was mainly caused by the booking scenario in the `k6` script generating many expected unsuccessful booking attempts under concurrent access, which `k6` still counts as failed HTTP requests.
- In other words, the run shows strong response-time performance, but the current load-test design inflates the failure metric. A better benchmark would isolate search traffic from booking-conflict traffic or use unique listing/date combinations per virtual user.

### Load Test Evidence

Add a screenshot or graph of the `k6` output here before final submission.

- Suggested file name: `docs/assets/k6-results.png`
- Suggested content: terminal output showing `avg`, `p95`, `requests/sec`, and `error rate`

## Issues Encountered

1. `GET /api/v1/listings/search` initially returned `500 Internal Server Error` because dates coming from Swagger were bound as `DateTimeKind.Unspecified`, while PostgreSQL (`timestamp with time zone`) expects UTC values.
2. The same date-handling issue could affect booking queries. It was fixed by normalizing incoming dates to UTC in the service layer before querying or saving.
3. Swagger UI behaved more reliably with `http://localhost:5001/api-docs/` including the trailing slash.

## Video Presentation

Suggested demo flow for a 3-5 minute recording:

1. Show the GitHub repository and README overview.
2. Show the architecture/data model section briefly.
3. Open deployed Swagger JSON or local Swagger and explain the available endpoints.
4. Demonstrate the main flow: register/login, create listing, query listings, book a stay, review a stay.
5. Show the Azure deployment URLs.
6. Show the `k6` load-test results and summarize the performance observations.

[Link to video presentation](<your-video-link>)
