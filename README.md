# News Portal API

Backend API untuk aplikasi **News Portal** yang menyediakan fitur autentikasi, artikel, komentar, dan like.

Project ini dibangun menggunakan ASP.NET Core Web API dengan ASP.NET Core Identity untuk authentication dan user management.

## Tech Stack

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* ASP.NET Core Identity
* JWT Authentication
* SQL Server
* Swagger / OpenAPI

## Features

* User registration & login
* JWT authentication
* Role-based authorization
* Article management
* Article comments
* Article likes
* User & role management

### Roles

| Role   | Access                            |
| ------ | --------------------------------- |
| User   | Read article, like, comment       |
| Author | Create, edit, delete own article  |
| Admin  | Manage articles, users, and roles |

## Getting Started

### Requirements

Pastikan sudah terinstall:

* [.NET SDK](https://dotnet.microsoft.com/download)
* SQL Server
* Git

### Clone Repository

```bash
git clone https://github.com/<username>/news-portal-api.git
cd news-portal-api
```

### Configure Database

Buat atau ubah connection string pada:

```text
src/NewsPortal.Api/appsettings.Development.json
```

Contoh:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NewsPortalDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Sesuaikan connection string dengan konfigurasi SQL Server di komputer Anda.

### Install Dependencies

```bash
dotnet restore
```

### Create Database

Jalankan Entity Framework Core migration:

```bash
dotnet ef database update
```

Jika `dotnet ef` belum tersedia:

```bash
dotnet tool install --global dotnet-ef
```

### Run Project

```bash
dotnet run --project src/NewsPortal.Api
```

Setelah aplikasi berjalan, buka Swagger pada URL yang ditampilkan oleh terminal, biasanya:

```text
https://localhost:<port>/swagger
```

Swagger dapat digunakan untuk mencoba endpoint API secara langsung.

## Project Structure

```text
src/
├── NewsPortal.Api
├── NewsPortal.Application
├── NewsPortal.Domain
└── NewsPortal.Infrastructure
```

## Authentication

API menggunakan JWT Authentication.

Setelah login, gunakan access token pada Swagger melalui tombol **Authorize**:

```text
Bearer <access-token>
```

## Database

Project menggunakan SQL Server dan Entity Framework Core Code First.

User management menggunakan ASP.NET Core Identity dengan `Guid` sebagai User ID.

## License

This project is for learning and portfolio purposes.
