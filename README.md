# EventHub

Advanced event management platform built with ASP.NET Core MVC, Entity Framework Core, Identity authentication and role-based authorization.

## Features

- Authentication with ASP.NET Identity
- Role-based access control: Admin, Organizer, User
- Event creation, editing and deletion
- Category management
- Ticket reservation with capacity checks
- Admin dashboard metrics: users, events, active events, registrations, revenue
- Search and filtering by title, location, category and price
- Responsive Bootstrap UI
- Local image upload for event covers
- Repository Pattern, Unit of Work, DTOs and service layer

## Screenshots

### Home page

![EventHub home page](docs/screenshots/eventhub-home.png)

### Events page

![EventHub events page](docs/screenshots/eventhub-events.png)

## Tech Stack

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core
- SQL Server-ready configuration
- SQLite local fallback for easy demo runs
- ASP.NET Core Identity
- Bootstrap, HTML, CSS, JavaScript

## Demo Accounts

All seeded accounts use this password:

```text
EventHub123!
```

```text
admin@eventhub.local
organizer@eventhub.local
user@eventhub.local
```

## Run Locally

```bash
dotnet restore EventHub.slnx
dotnet build EventHub.slnx
dotnet run --project EventHub.Web/EventHub.Web.csproj --no-launch-profile --urls http://localhost:5088
```

Open:

```text
http://localhost:5088
```

## Database Provider

The project runs with SQLite by default so it works immediately on machines without SQL Server LocalDB.

To use SQL Server, update `EventHub.Web/appsettings.json`:

```json
{
  "DatabaseProvider": "SqlServer"
}
```

Then set `DefaultConnection` to your SQL Server connection string.

## Architecture

```text
EventHub.Web
├── Areas/Admin
├── Constants
├── Controllers
├── Data
├── DTOs
├── Interfaces
├── Models
├── Repositories
├── Services
├── ViewModels
├── Views
└── wwwroot
```

## CV Description

EventHub — Advanced Event Management Platform

Built a full-featured event management platform using ASP.NET Core MVC, Entity Framework Core, Identity Authentication and role-based authorization. Implemented event registration, category management, admin dashboard analytics, responsive UI, filtering, image upload and clean service/repository architecture.
