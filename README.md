# News Portal

Website portal berita berbasis **ASP.NET Core MVC** yang memungkinkan pengguna membaca artikel, memberikan like, dan memberikan komentar.

Terdapat tiga role utama:

* **User** — membaca artikel, like, dan komentar
* **Author** — membuat dan mengelola artikel miliknya
* **Admin** — mengelola artikel, kategori, user, dan role

## Tech Stack

* ASP.NET Core MVC / .NET
* ASP.NET Core Identity
* Entity Framework Core
* PostgreSQL
* Bootstrap 5
* Razor Views

## Requirements

Pastikan sudah terinstall:

* [.NET SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL](https://www.postgresql.org/download/)
* Git

## Setup

### 1. Clone Repository

```bash
git clone https://github.com/jaguardeva/news-portal.git
cd news-portal
```

### 2. Configure User Secrets

Project menggunakan **.NET User Secrets** untuk menyimpan credential development seperti connection string dan authentication secret.

Jalankan dari folder project:

```bash
dotnet user-secrets init
```

Kemudian tambahkan connection string PostgreSQL:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=NewsPortal;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
```

> Secret tidak perlu disimpan di `appsettings.json` atau di-upload ke GitHub.

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Update Database

```bash
dotnet ef database update
```

Jika Entity Framework CLI belum tersedia:

```bash
dotnet tool install --global dotnet-ef
```

Kemudian jalankan kembali:

```bash
dotnet ef database update
```

### 5. Run Application

```bash
dotnet run
```

Atau jalankan melalui Visual Studio / Rider.

Setelah aplikasi berjalan, buka URL yang ditampilkan pada terminal.

## Development Configuration

Konfigurasi umum aplikasi berada di:

```text
appsettings.json
```

Konfigurasi khusus development dapat menggunakan:

```text
appsettings.Development.json
```

Jangan menyimpan password, connection string yang mengandung credential, API key, atau secret lainnya di dalam repository.

Untuk credential lokal gunakan:

```bash
dotnet user-secrets list
```

## Database

Database menggunakan **PostgreSQL** dan dikelola dengan **Entity Framework Core Code First**.

Authentication dan user management menggunakan **ASP.NET Core Identity** dengan `Guid` sebagai User ID.

## License

This project is for learning and portfolio purposes.
