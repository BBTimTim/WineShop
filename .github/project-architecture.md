# Winery Architecture and Project Guide

## 1. Executive Overview

Winery is an online wine store built with ASP.NET Core MVC. It supports both customer-facing shopping features and a protected administration area.

The application allows customers to:

- Browse active wine products.
- Filter products by category and brand.
- Search and sort products.
- View product details, images, specifications, reviews, prices, and stock information.
- Add products to a shopping cart.
- Maintain a wishlist.
- Register, sign in, and manage an account.
- Place orders and review order history.
- Track orders.
- Complete payments through Stripe.

Administrators can manage products, brands, categories, orders, inventory, reports, images, product specifications, and Excel import/export operations.

## 2. Technology and Runtime

| Area | Technology | Purpose |
|---|---|---|
| Runtime | .NET 10 | Application runtime and target framework |
| Web framework | ASP.NET Core MVC | HTTP request handling, routing, controllers, and views |
| UI | Razor views | Server-rendered HTML pages |
| ORM | Entity Framework Core 10 | Database access and entity mapping |
| Database | SQL Server | Persistent application data |
| Authentication | ASP.NET Core Identity | Users, passwords, claims, and roles |
| Payments | Stripe.net | Stripe payment processing |
| Object mapping | AutoMapper 17 | Mapping between compatible object types |
| Spreadsheet processing | ClosedXML | Excel report export and product import |
| Styling and client scripts | CSS, JavaScript, Bootstrap, jQuery | Browser-side presentation and behavior |

The project uses nullable reference types and implicit usings. The main project file is `Winery.csproj`, and the application is named Winery.

## 3. Application Startup and Request Pipeline

`Program.cs` is the composition root of the application. It configures services, middleware, authentication, authorization, routing, and application startup tasks.

### 3.1 Service registration

The application registers:

- MVC controllers and Razor views with `AddControllersWithViews()`.
- `Winery.Data.DbContext` with SQL Server and the `DefaultConnection` connection string.
- ASP.NET Core Identity using the custom `User` entity and `IdentityRole`.
- Entity Framework stores for Identity.
- Authorization policies named `AdminOnly` and `CustomerOnly`.
- HTTP context access.
- Distributed memory cache and session state.
- Stripe settings from the `Stripe` configuration section.
- Product, shopping cart, file, email, order, and report services.
- AutoMapper mappings from the current assembly.

### 3.2 Middleware order

The request pipeline currently performs these operations:

1. Applies the production exception handler and HSTS outside Development.
2. Redirects HTTP requests to HTTPS.
3. Serves static files from `wwwroot`.
4. Enables endpoint routing.
5. Enables session state.
6. Configures the Stripe API key.
7. Enables authentication.
8. Enables authorization.
9. Maps the Admin area route.
10. Maps the default customer-facing route.

Authentication must run before authorization. Session must be enabled before services or controllers attempt to read shopping cart session data.

### 3.3 Startup database initialization

`DbInitializer.SeedAsync` runs during application startup. It applies database initialization tasks and ensures required Identity roles or seed data are available before requests are handled.

## 4. Layered Architecture

The application follows a conventional ASP.NET Core MVC layered structure:

```text
HTTP request
	-> Route
	-> Controller
	-> Service
	-> Entity Framework DbContext
	-> SQL Server
	-> View model or domain model
	-> Razor view
	-> HTML response
```

### 4.1 Controllers

Controllers receive HTTP requests, validate request-level input, invoke services, select views, and return redirects or responses.

Controllers should not contain large amounts of business logic or duplicated database queries. New reusable business operations should be placed in a service and exposed through an `I*Service` interface.

Customer-facing controllers are located in `Controllers/`. Administrative controllers are located in `Areas/Admin/Controllers/`.

### 4.2 Services

Services contain reusable business operations and coordinate database access, file handling, orders, shopping carts, reports, and email.

Existing service abstractions include:

- `IProductService` – product queries, filtering, featured products, new arrivals, bestsellers, related products, brands, and reviews.
- `IShoppingCartService` – shopping cart operations.
- `IFileService` – saving and deleting uploaded files.
- `IOrderService` – order creation and order-related operations.
- `IReportService` – dashboard statistics, inventory, sales, and top-product reports.
- `IEmailService` – email-related operations.

Services are registered with scoped lifetime in `Program.cs` and normally receive the database context through constructor injection.

### 4.3 Models

`Models/` contains Entity Framework entities and domain models. These classes represent persistent data and relationships, including products, categories, brands, users, orders, order items, reviews, images, specifications, cart items, and wishlist items.

Data annotations such as `[Required]`, `[StringLength]`, `[Display]`, and `[Column]` define validation, labels, and database column details where appropriate.

### 4.4 ViewModels

`ViewModels/` contains models designed for a particular page or operation. View models should be preferred when a page needs a combination of filters, lookup data, pagination information, form input, or display-only values.

A view model helps avoid exposing unnecessary domain properties directly to a form and prevents views from depending on database implementation details.

### 4.5 Razor views

`Views/` contains customer-facing Razor views. `Areas/Admin/Views/` contains administrative views.

Views should be responsible for presentation only. They should display model data, render forms, and use existing partial views or view components where appropriate. Database queries and business rules should not be added directly to views.

## 5. Data Access and Entity Relationships

`Data/DbContext.cs` defines the application database context. It inherits from `IdentityDbContext<User>`, so it contains both application entities and ASP.NET Core Identity tables.

The context exposes these main `DbSet` properties:

- `Categories`
- `Brands`
- `Products`
- `ProductImages`
- `ProductSpecifications`
- `Reviews`
- `Orders`
- `OrderItems`
- `ShoppingCartItems`
- `WishlistItems`

### 5.1 Product relationships

A product belongs to one category and one brand. A product can contain:

- Multiple product images.
- Multiple specifications.
- Multiple reviews.
- Multiple order items.
- Multiple wishlist entries.

Category and brand deletion uses restricted behavior to prevent deleting records that are still referenced by products.

### 5.2 Database indexes

The context defines indexes for frequently queried fields, including:

- Product name.
- Product featured, new-arrival, and bestseller flags.
- Order number, status, and order date.
- Review product and user identifiers.
- Shopping cart identifier.

### 5.3 Querying rules

Use EF Core asynchronous methods for database operations:

- `FirstOrDefaultAsync` for an optional single result.
- `SingleOrDefaultAsync` only when uniqueness is guaranteed.
- `AnyAsync` for existence checks.
- `CountAsync` for counts.
- `ToListAsync` for materializing collections.
- `SaveChangesAsync` for persistence.

Use `Include` and `ThenInclude` when a page requires related data. Filter inactive products in customer-facing queries unless the operation is explicitly an administrative operation.

Avoid using an `IQueryable` directly as a Boolean condition. Use `AnyAsync` or `CountAsync` instead.

## 6. Customer-Facing Features

### 6.1 Product catalog

The product service builds an `IQueryable<Product>` and applies optional filters for:

- Category.
- Brand.
- Search text.
- Minimum price.
- Maximum price.
- Sort order.

The query is paginated with `Skip` and `Take`. The resulting `ProductListViewModel` contains products, filter values, lookup collections, and pagination information.

### 6.2 Product details

The product details operation loads the product with its brand, category, images, specifications, reviews, and review users when those relationships are required by the page.

Related products are selected using matching brand or category information while excluding the current product and inactive products.

### 6.3 Reviews

Authenticated users can submit product reviews through the product service. Review input should be validated before persistence, and the current authenticated user identity must be used rather than accepting an arbitrary user identifier from the browser.

### 6.4 Shopping cart

The shopping cart service manages cart entries and quantities. Session support is configured in `Program.cs`, and cart operations should preserve stock and product availability rules.

### 6.5 Accounts and orders

ASP.NET Core Identity manages user registration, login, password changes, password reset tokens, and roles. Order features use the current authenticated user and should not allow one user to view or modify another user's orders.

### 6.6 Payments

Stripe settings are bound from configuration and the Stripe secret key is assigned at startup. Stripe secrets must come from User Secrets or environment variables and must never be hard-coded or committed.

## 7. Administration Area

The administration area is located under `Areas/Admin/` and uses the `Admin` area route.

Administrative controllers should use role-based authorization, normally with `[Authorize(Roles = "Admin")]`. Mutating actions should use both `[HttpPost]` and `[ValidateAntiForgeryToken]`.

The administration area includes functionality for:

- Product listing, creation, editing, soft deletion, and details.
- Product image upload and removal.
- Product specifications.
- Category management.
- Brand management.
- Order management and order details.
- Dashboard statistics.
- Inventory reports.
- Sales reports.
- Top-product reports.
- Product Excel export and import.

Product deletion currently follows a soft-delete pattern by setting `IsActive` to `false`. Customer-facing queries must respect this flag.

## 8. File Storage and Static Assets

`IFileService` handles uploaded files and removes stored files when needed. Product and other uploaded images are stored below `wwwroot/Images/` or the configured upload path.

The `wwwroot/` directory contains:

- CSS files in `wwwroot/css/`.
- JavaScript files in `wwwroot/js/`.
- Product, brand, category, and profile image directories.
- Client-side libraries under `wwwroot/lib/`.

Uploaded file names, extensions, sizes, and content should be validated before saving. Do not trust a browser-provided file name or path.

## 9. Configuration and Secrets

Configuration is stored in:

- `appsettings.json` – shared defaults.
- `appsettings.Development.json` – development-specific settings.
- User Secrets or environment variables – local secrets and credentials.

Sensitive values include:

- SQL Server connection strings.
- Stripe secret keys.
- External authentication credentials.
- Email service credentials.

Never commit sensitive values. Configuration should be injected through `IConfiguration`, options classes, or the existing `StripeSettings` pattern.

## 10. Authentication and Authorization

The application uses ASP.NET Core Identity with the custom `User` entity and `IdentityRole`.

The configured password rules require:

- At least six characters.
- A digit.
- A lowercase character.
- An uppercase character.
- No mandatory non-alphanumeric character.

The application defines `AdminOnly` and `CustomerOnly` authorization policies. Prefer the existing policies or role-based authorization attributes rather than duplicating authorization logic.

Authorization must be checked server-side for every protected operation. Hiding a link or button in a Razor view is not sufficient protection.

## 11. Validation and Security Rules

- Use model binding allowlists such as `[Bind]` for form posts where appropriate.
- Validate all user-controlled values on the server.
- Keep antiforgery protection on state-changing form actions.
- Use authorization checks before reading or changing user-owned resources.
- Do not trust product prices, quantities, user IDs, order IDs, or roles submitted by a client.
- Use server-side product and stock data when calculating orders.
- Avoid logging passwords, tokens, payment secrets, or full sensitive user data.
- Use structured logging through `ILogger<T>`.
- Return `NotFound()` when a requested resource does not exist or is not accessible.

## 12. Entity Framework Migrations

Database schema changes are stored under `Migrations/`. When changing an entity, relationship, property, index, or database configuration:

1. Review the current model and migration snapshot.
2. Determine whether a migration is required.
3. Create a migration using the project's EF Core tooling.
4. Review the generated migration before applying it.
5. Build the project and verify that the application starts correctly.

Do not edit an existing applied migration to change database history. Add a new migration for a new model change.

## 13. Development Workflow

For a normal code change:

1. Locate the relevant controller, service, model, view model, and Razor view.
2. Check existing interfaces and dependency injection registrations.
3. Keep business logic in a service where it can be reused and tested.
4. Use asynchronous database access.
5. Preserve authorization and antiforgery protection.
6. Update views and view models only when the page contract changes.
7. Add or update a migration if the data model changes.
8. Build the project.
9. Run relevant tests if they are available.
10. Review the final change for secrets, authorization gaps, and unnecessary package additions.

## 14. Validation Commands

Build the project with:

```powershell
dotnet build Winery.csproj
```

Useful Entity Framework commands include:

```powershell
dotnet ef migrations add <MigrationName> --project Winery.csproj
dotnet ef database update --project Winery.csproj
```

Only run database update commands against the intended environment and database.

## 15. Coding Conventions

- Follow the existing C# formatting and naming conventions.
- Use PascalCase for public types and members.
- Use meaningful names for services, actions, variables, and view models.
- Prefer constructor dependency injection.
- Keep controller actions focused on HTTP concerns.
- Prefer explicit, readable asynchronous code over compressed expressions when behavior is non-trivial.
- Reuse existing services, models, partial views, and components before creating duplicates.
- Do not introduce a new NuGet package when an existing dependency or framework feature is sufficient.
- Keep changes narrowly scoped to the requested task.
- Add comments only when they explain non-obvious behavior or an important security or data constraint.

## 16. Important Project Facts

- The project uses ASP.NET Core MVC with Razor views, not Blazor.
- The main project file is `Winery.csproj`.
- The default database context is `Winery.Data.DbContext`.
- Administrative functionality belongs under the `Admin` area.
- Customer-facing product queries should normally exclude inactive products.
- The application uses SQL Server through Entity Framework Core.
- Secrets must be supplied through User Secrets or environment variables.
