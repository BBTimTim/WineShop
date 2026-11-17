# Project Description

## Project Purpose
Winery is an online wine store. The system provides product browsing, category and brand filtering, shopping cart management, wishlists, order management, user accounts, and administration features.

## Technology Stack
- .NET 10 and ASP.NET Core
- ASP.NET Core MVC with Razor views
- Entity Framework Core 10
- SQL Server database
- ASP.NET Core Identity for user and role management
- Stripe payment integration
- AutoMapper 17
- ClosedXML for Excel export and import
- Nullable reference types and implicit usings enabled

## Project Structure
- `Controllers/` – controllers for the customer-facing site
- `Areas/Admin/Controllers/` – administration controllers
- `Areas/Admin/Views/` – administration Razor views
- `Views/` – customer-facing Razor views
- `Models/` – Entity Framework entities and domain models
- `ViewModels/` – view models
- `Services/` – business logic, file handling, orders, products, cart, and reporting
- `Data/` – DbContext and database initialization
- `Migrations/` – Entity Framework Core migrations
- `wwwroot/` – static CSS, JavaScript, images, and client-side libraries

## Important Development Rules
- Follow the existing layered architecture when making changes.
- Keep business logic in the `Services/` directory rather than in controllers or Razor views whenever possible.
- Use asynchronous EF Core methods for database queries, such as `FirstOrDefaultAsync`, `ToListAsync`, `AnyAsync`, and `SaveChangesAsync`.
- Validate user input and preserve antiforgery protection for mutating administration operations.
- Administration endpoints belong in the `Admin` area and must use role-based authorization.
- Use the existing `I*Service` interfaces and dependency injection registrations when adding business functionality.
- Do not add a new NuGet package if the task can be solved with existing dependencies.
- Keep changes minimal and consistent with the existing code style.

## Configuration
Environment-specific settings are stored in `appsettings.json` and `appsettings.Development.json`. Do not put secrets such as database connection strings or Stripe keys in source code or committed configuration files; use User Secrets or environment variables for development.

## Validation
After code changes, build the project:

```powershell
dotnet build Winery.csproj
```

When changing the data model, also check whether an Entity Framework Core migration is required.
