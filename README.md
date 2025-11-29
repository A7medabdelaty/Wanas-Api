# Wanas-Api

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=flat&logo=csharp)
![License](https://img.shields.io/badge/license-MIT-green?style=flat)
![GitHub](https://img.shields.io/badge/github-Wanas--Api-lightgrey?style=flat&logo=github)

> A comprehensive .NET 9 ASP.NET Core Web API platform for apartment listing management and intelligent roommate matching, featuring AI-powered descriptions, real-time messaging, advanced search capabilities, and complete admin moderation.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Project Architecture](#project-architecture)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Development Guide](#development-guide)
- [Deployment](#deployment)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

---

## Overview

**Wanas-Api** is an enterprise-grade apartment rental platform built with .NET 9 and ASP.NET Core. It provides a complete solution for property owners to list apartments and for tenants to find compatible roommates through intelligent AI-powered matching algorithms.

### Core Capabilities

- 🏠 **Comprehensive Listing Management** — Create, manage, and showcase apartment properties with multi-photo uploads, detailed room specifications, and amenity tracking
- 🤖 **AI-Powered Matching** — Intelligent roommate matching algorithm analyzing lifestyle compatibility, preferences, and schedule alignment
- 💬 **Real-time Communication** — SignalR-powered instant messaging with read receipts and typing indicators
- 🔍 **Advanced Search** — Semantic search with vector embeddings, dynamic filtering, sorting, and pagination
- 🧠 **AI Integration** — OpenAI and Groq APIs for auto-generating listing descriptions and AI chatbot assistance
- 🛡️ **Moderation Suite** — Comprehensive admin tools for managing reviews, appeals, reports, and user verification
- 💳 **Payment Processing** — Integrated payment handling, commission calculation, and payout management
- 📊 **Analytics & Reporting** — Real-time metrics, revenue tracking, user engagement analytics, and KPI monitoring
- 🔐 **Enterprise Security** — JWT-based authentication with role-based access control (RBAC) and audit logging

---

## Key Features

| Feature | Description | Use Case |
|---------|-------------|----------|
| **Apartment Management** | CRUD operations for listings with photos, room details, amenities, and pricing | Property owners managing rental properties |
| **Roommate Matching** | AI algorithm matching users based on lifestyle, preferences, and compatibility scores | Tenants finding ideal roommates |
| **Real-time Chat** | SignalR-based messaging with read receipts and presence detection | Instant communication between users |
| **Vector Search** | Semantic search using embeddings with Chroma for intelligent discoverability | Users finding apartments matching their preferences |
| **AI Assistance** | Auto-generate compelling descriptions and provide AI chatbot support | Faster listing creation and user support |
| **Moderation System** | Review management, appeal handling, user reporting, and content flagging | Maintaining platform safety and quality |
| **Payment System** | Secure transactions, commission tracking, and automated payouts | Platform monetization and financial management |
| **Analytics Dashboard** | Traffic metrics, revenue tracking, user engagement, and platform KPIs | Business intelligence and decision making |
| **JWT Authentication** | Token-based auth with multiple roles and permission levels | Secure API access and user sessions |

---

## Tech Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Language** | C# | 13.0 | Modern language with advanced features |
| **Framework** | ASP.NET Core | .NET 9 | High-performance web framework |
| **API** | ASP.NET Core Web API | 9.0 | RESTful API development |
| **Database ORM** | Entity Framework Core | 9.0 | Object-relational mapping |
| **Database** | SQL Server / PostgreSQL | 2019+ / 12+ | Relational data persistence |
| **Real-time** | SignalR | 9.0 | WebSocket communication |
| **Validation** | FluentValidation | Latest | Rule-based input validation |
| **Mapping** | AutoMapper | Latest | DTO-to-Entity conversion |
| **Messaging** | SignalR | 9.0 | Real-time bidirectional communication |
| **Vector Search** | Chroma DB | Latest | Semantic search with embeddings |
| **AI Models** | OpenAI API / Groq | Latest | Large language models |
| **Authentication** | JWT (JSON Web Tokens) | Standard | Stateless authentication |
| **Caching** | Distributed Cache | 9.0 | Performance optimization |
| **Architecture Pattern** | CQRS + Clean Architecture | Best Practice | Separation of concerns |

---

## Prerequisites

### System Requirements

| Requirement | Minimum | Recommended | Notes |
|------------|---------|------------|-------|
| **.NET SDK** | 9.0 | 9.x LTS | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **Database** | SQL Server 2019 or PostgreSQL 12 | SQL Server 2022 or PostgreSQL 15+ | For data persistence |
| **Memory** | 4 GB | 8+ GB | For development and testing |
| **Disk Space** | 2 GB | 5+ GB | For NuGet packages and database |
| **Git** | 2.0+ | Latest | For version control |
| **IDE** | Visual Studio Code | Visual Studio 2026 | For development (optional) |

### Required Services (Optional but Recommended)

- **Email Service** — SMTP server for email notifications (Gmail, SendGrid, etc.)
- **OpenAI API** — For AI-powered listing descriptions (optional)
- **Groq API** — Alternative LLM provider for AI features (optional)
- **Chroma DB** — For vector embeddings and semantic search (optional)

---

## Quick Start

### 1. Clone Repository

```bash
git clone https://github.com/A7medabdelaty/Wanas-Api.git
cd Wanas-Api
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Configure Database

Edit `Wanas.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=WanasDb;Trusted_Connection=true;"
  }
}
```

**Connection String Examples:**

```bash
# SQL Server (Windows Authentication)
Server=.;Database=WanasDb;Trusted_Connection=true;

# SQL Server (SQL Authentication)
Server=localhost;Database=WanasDb;User Id=sa;Password=YourPassword;Encrypt=true;TrustServerCertificate=true;

# PostgreSQL
Host=localhost;Port=5432;Database=wanasdb;Username=postgres;Password=YourPassword;
```

### 4. Apply Migrations

```bash
dotnet ef database update --project Wanas.Infrastructure --startup-project Wanas.API
```

### 5. Run Application

```bash
dotnet run --project Wanas.API
```

**Access Points:**
- API: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger` (if configured)
- SignalR Hub: `wss://localhost:5001/hubs/chat`

---

## Project Architecture

### Layered Architecture Diagram

```
┌───────────────────────────────────────────────────────-─┐
│                  Wanas.API                              │
│      Controllers · Hubs · Middleware · Authorization    │
│           (HTTP Request/Response Layer)                 │
└──────────────────────┬─────────────────────────────────-┘
                       │ depends on
                       ▼
┌────────────────────────────────────────────────────────-┐
│              Wanas.Application                          │
│   Services · DTOs · CQRS Handlers · Validators · Maps   │
│          (Business Logic & Orchestration Layer)         │
└──────────────────────┬─────────────────────────────────-┘
                       │ depends on
                       ▼
┌────────────────────────────────────────────────────────-┐
│             Wanas.Infrastructure                        │
│  Repositories · DbContext · Auth · External Integrations|
│        (Technical Implementation Layer)                 │
└──────────────────────┬─────────────────────────────────-┘
                       │ depends on
                       ▼
┌──────────────────────────────────────────────────────-──┐
│                 Wanas.Domain                            │
│    Entities · Enums · Exceptions · Domain Events        │
│      (Business Rules Layer - No Dependencies)           │
└──────────────────────────────────────────────────────-──┘
```

### Architecture Principles

✅ **Clean Architecture** — Clear separation of concerns with dependency inversion  
✅ **Domain-Driven Design** — Business rules centralized in domain layer  
✅ **CQRS Pattern** — Read and write operations clearly separated  
✅ **Dependency Injection** — Loose coupling through constructor injection  
✅ **Repository Pattern** — Data access abstraction for testability  
✅ **Service Layer** — Business logic orchestration through services  
✅ **DTO Pattern** — Request/response separation from domain models  
✅ **Validation** — Centralized FluentValidation on all inputs  
✅ **Async/Await** — Asynchronous operations for scalability  
✅ **Error Handling** — Consistent exception handling and result patterns  

---

## Project Structure

### Directory Layout

```
Wanas-Api/
│
├── Wanas.Domain/
│   ├── Entities/              ← Domain models
│   ├── Repositories/          ← Repository interfaces
│   ├── Enums/                 ← Domain enumerations
│   ├── Errors/                ← Error definitions
│   ├── Events/                ← Domain events
│   ├── Exceptions/            ← Custom exceptions
│   └── Models/                ← Supporting domain models
│
├── Wanas.Application/
│   ├── DTOs/                  ← Data Transfer Objects
│   ├── Services/              ← Business logic services
│   ├── Interfaces/            ← Service contracts
│   ├── Handlers/              ← CQRS handlers
│   ├── Commands/              ← CQRS commands
│   ├── Queries/               ← CQRS queries
│   ├── Validators/            ← Validation rules
│   ├── Mappings/              ← AutoMapper profiles
│   ├── QueryBuilders/         ← Query builders
│   └── Helpers/               ← Utility helpers
│
├── Wanas.Infrastructure/
│   ├── Persistence/           ← DbContext & configurations
│   ├── Repositories/          ← Repository implementations
│   ├── Migrations/            ← Database migrations
│   ├── Authentication/        ← JWT provider
│   ├── Services/              ← External service implementations
│   ├── AI/                    ← AI providers (OpenAI, Groq)
│   ├── Caching/               ← Caching implementations
│   └── Settings/              ← Configuration options
│
├── Wanas.API/
│   ├── Controllers/           ← API endpoints
│   ├── Hubs/                  ← SignalR hubs
│   ├── Middleware/            ← Custom middleware
│   ├── Authorization/         ← Authorization handlers
│   ├── Responses/             ← Response wrappers
│   ├── Extensions/            ← Extension methods
│   ├── appsettings.json       ← Configuration (dev)
│   ├── appsettings.Production.json ← Configuration (prod)
│   └── Program.cs             ← Startup configuration
│
└── README.md
```

### Layer Responsibilities

| Layer | Responsibility | Dependencies | Examples |
|-------|-----------------|----------------|----------|
| **Domain** | Core business rules and entities | None | `Listing`, `Room`, `Bed`, `User`, `Report` |
| **Application** | Use cases, orchestration, validation | Domain | `ListingService`, `MatchingService`, `AuthService` |
| **Infrastructure** | Data access, external integrations | Domain, Application | `ListingRepository`, `EmailService`, `JwtProvider` |
| **API** | HTTP interface and routing | All layers | `ListingController`, `ChatsController`, `AdminUsersController` |

---

## Configuration

### Environment Setup

#### Development (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=WanasDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Key": "your-development-secret-key-minimum-32-characters-long",
    "Issuer": "wanas-api",
    "Audience": "wanas-users",
    "ExpirationMinutes": 60
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-key"
  },
  "Chroma": {
    "Host": "localhost",
    "Port": 8000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## API Documentation

### Authentication Endpoints

| Method | Endpoint | Description | Body |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | `RegisterRequest` |
| POST | `/api/auth/login` | User login | `LoginRequest` |
| POST | `/api/auth/refresh-token` | Refresh JWT token | `RefreshTokenRequest` |
| POST | `/api/auth/confirm-email` | Confirm email | `ConfirmEmailRequest` |
| POST | `/api/auth/resend-confirmation` | Resend email verification | `ResendConfirmationRequest` |
| POST | `/api/auth/forget-password` | Request password reset | `ForgetPasswordRequest` |
| POST | `/api/auth/reset-password` | Reset password | `ResetPasswordRequest` |

### User Management Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/user/profile` | Get user profile |
| PUT | `/api/user/profile` | Update user profile |
| POST | `/api/user/complete-profile` | Complete profile setup |
| GET | `/api/user/preferences` | Get user preferences |
| PUT | `/api/user/preferences` | Update preferences |

### Apartment Listing Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/listings` | List all apartments (paginated) |
| GET | `/api/listings/{id}` | Get apartment details |
| POST | `/api/listings` | Create new listing |
| PUT | `/api/listings/{id}` | Update listing |
| DELETE | `/api/listings/{id}` | Delete listing |
| POST | `/api/listings/{id}/photos` | Upload photos |
| GET | `/api/listings/{id}/rooms` | Get rooms |
| POST | `/api/listings/{id}/rooms` | Add room |
| PUT | `/api/listings/{id}/rooms/{roomId}` | Update room |
| DELETE | `/api/listings/{id}/rooms/{roomId}` | Delete room |

### Search & Matching Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/listings-search/search` | Advanced search with filters |
| POST | `/api/matching/find-roommates` | Find compatible roommates |

### Chat & Messages Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/chats` | Get user conversations |
| POST | `/api/chats` | Create new chat |
| GET | `/api/chats/{id}/messages` | Get messages in chat |
| POST | `/api/messages` | Send message |
| PUT | `/api/messages/{id}` | Edit message |
| DELETE | `/api/messages/{id}` | Delete message |
| **WebSocket** | `wss://localhost:5001/hubs/chat` | Real-time chat hub |

### Review Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/reviews` | Create review |
| PUT | `/api/reviews/{id}` | Update review |
| DELETE | `/api/reviews/{id}` | Delete review |
| GET | `/api/reviews/listing/{listingId}` | Get listing reviews |

### Report & Appeal Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/reports` | Submit report |
| GET | `/api/reports/{id}` | Get report |
| POST | `/api/appeals` | Submit appeal |
| GET | `/api/appeals` | Get user appeals |

### AI Features Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/ai/generate-listing` | Generate listing description |
| POST | `/api/chatbot/chat` | Chat with AI assistant |

### Admin Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/users` | List all users |
| POST | `/api/admin/users/{id}/verify` | Verify user |
| POST | `/api/admin/users/{id}/ban` | Ban user |
| POST | `/api/admin/users/{id}/suspend` | Suspend user |
| GET | `/api/admin/listings/moderation` | Pending moderation |
| GET | `/api/admin/reports` | List reports |
| GET | `/api/admin/appeals` | List appeals |
| GET | `/api/admin/analytics` | Get analytics |
| GET | `/api/admin/revenue` | Get revenue data |

---

## Development Guide

### Code Style & Conventions

**Follow these standards throughout the codebase:**

- Use **PascalCase** for types, public methods, and properties
- Use **camelCase** for parameters, local variables, and private members
- Use **SCREAMING_SNAKE_CASE** for constants
- Prefix interfaces with `I` (e.g., `IListingService`)
- Prefix private fields with `_` (e.g., `_userRepository`)
- Write XML documentation for all public members
- Keep lines under 120 characters for readability
- Use meaningful names; avoid abbreviations
- Maximum 3 levels of nesting for complexity
- Maximum 20 lines per method (aim for single responsibility)

### Database Migrations

**Create migration:**
```bash
dotnet ef migrations add MigrationName --project Wanas.Infrastructure --startup-project Wanas.API
```

**Apply migrations:**
```bash
dotnet ef database update --project Wanas.Infrastructure --startup-project Wanas.API
```

**Revert migration:**
```bash
dotnet ef database update PreviousMigrationName --project Wanas.Infrastructure --startup-project Wanas.API
```

**List migrations:**
```bash
dotnet ef migrations list --project Wanas.Infrastructure --startup-project Wanas.API
```

### Building & Publishing

**Build solution:**
```bash
dotnet build
dotnet build -c Release  # Production build
```

**Run tests:**
```bash
dotent test
dotnet test --collect:"XPlat Code Coverage"  # With coverage
```

**Publish application:**
```bash
dotnet publish -c Release -o ./publish
```

### Project Organization Guidelines

1. **Keep projects focused** — Each project has a single responsibility
2. **Maintain dependency flow** — Always depend downward (API → Application → Infrastructure → Domain)
3. **Use abstractions** — Interfaces in Application/Domain, implementations in Infrastructure
4. **Organize by feature** — Group related DTOs, Services, Handlers together
5. **Document assumptions** — Add XML docs explaining why, not just what

---

## Contributing

We welcome contributions! Please follow these guidelines:

### Contribution Process

1. **Fork** the repository on GitHub
2. **Clone** your fork locally
3. **Create** a feature branch: `git checkout -b feature/your-feature-name`
4. **Make** your changes following code style
5. **Test** your changes: `dotnet test`
6. **Commit** with descriptive messages: `git commit -m "Add: feature description"`
7. **Push** to your fork: `git push origin feature/your-feature-name`
8. **Open** a Pull Request with detailed description

### Pull Request Guidelines

- Link related issues
- Describe what the PR accomplishes
- Include before/after screenshots if UI changes
- Ensure all tests pass
- Follow code style and conventions
- Update documentation as needed

### Code Review Process

- At least one approval required
- All CI/CD checks must pass
- Merge only after approval

See `CONTRIBUTING.md` for detailed guidelines.

---

## License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) file for details.

You are free to:
- ✅ Use commercially and non-commercially
- ✅ Modify the source code
- ✅ Distribute the software
- ✅ Use private modifications

With the condition:
- ⚠️ Include the original license and copyright notice

---

## Support

### Getting Help

- **📚 Documentation** — See README and code comments for guidance
- **🐛 Bug Reports** — [GitHub Issues](https://github.com/A7medabdelaty/Wanas-Api/issues)
- **💡 Feature Requests** — [GitHub Discussions](https://github.com/A7medabdelaty/Wanas-Api/discussions)
- **📧 Email Support** — [ahmed.abdelaty174@gmail.com](mailto:ahmed.abdelaty174@gmail.com)

### Useful Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/signalr)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

### Community

- **GitHub**: [@A7medabdelaty](https://github.com/A7medabdelaty)
- **Issues**: [Report bugs](https://github.com/A7medabdelaty/Wanas-Api/issues)
- **Discussions**: [Share ideas](https://github.com/A7medabdelaty/Wanas-Api/discussions)

---

## Acknowledgments

Built with ❤️ by [A7medabdelaty](https://github.com/A7medabdelaty)

Special thanks to the open-source community and all contributors.

---

## Project Status

| Status | Details |
|--------|---------|
| **Current Version** | 1.0.0 |
| **Latest Release** | [View Releases](https://github.com/A7medabdelaty/Wanas-Api/releases) |
| **Build Status** | Active Development |
| **Maintenance** | Actively Maintained |
| **Support Level** | Community Supported |

---

**Happy coding! 🚀**