# Library Management

A full-stack library management application built with Angular 17 and .NET 8 Web API. This project was created to enable the management of an online library and allow users to request books.


## Technologies

- **Frontend**: Angular 17, Angular Material, RxJS
- **Backend**: ASP.NET Core 8, Entity Framework Core, Dapper (for initialization)
- **Database**: SQLite
- **Authentication**: Google OAuth 2.0 & Custom JWT
- **Technology**: Uses Dapper only to run the scripts.

## Features

- **Books Management**: CRUD operations for books with pagination and search.
- **Clients Management**: Manage library users.
- **Loans Management**: Track book loans and returns.
- **Categories**: Organize books by category.
- **Authentication**: Secure login via Google or Email/Password.
- **Internationalization**: Support for Multiple Languages (English/Portuguese).

## Getting Started

### Backend
Navigate to the `LibraryBackend` folder and run:
```bash
dotnet run
```
The backend initializes the SQLite database (`library.db`) automatically using scripts in `SqlScripts/Creation`.

### Frontend
Navigate to the `LibraryFrontend` folder and run:
```bash
ng serve
```
Navigate to `http://localhost:55396/` (or the port shown in console).

## Database Updates
The application checks the `DbVersion` table and looks for new scripts in `SqlScripts/Updates/` at startup to apply schema changes automatically.

The SqlScripts/Creation/initial_schema.sql file is executed if the database does not exist.

Updates: The application checks the DbVersion table and looks for new scripts in SqlScripts/Updates/ at startup.

Expected format for updates: 2_AddColumn.sql, 3_NewTable.sql, etc.
