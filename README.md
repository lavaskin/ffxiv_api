# FFXIV API

A simple .NET 8 Web API for FFXIV data with SQL Server integration.

There is *currently* no authentication, as it was built under the assumption I'll just be running it locally for myself. Everything is insecure, blah blah blah

## Prerequisites

- .NET 8 SDK
- SQL Server access (tested with SQL Server 2019+)

## SQL Server Authentication Setup

This project uses SQL Server authentication. Connection strings are **never committed to the repository** for security.

### Development Environment

The project uses **User Secrets** to store sensitive configuration like database connection strings locally.

#### Initial Setup

1. Initialize user secrets (already done if cloning this repo):
   ```bash
   dotnet user-secrets init
   ```

2. Set your SQL connection string:
   ```bash
   dotnet user-secrets set "SQL:ConnectionString" "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
   ```

   Example:
   ```bash
   dotnet user-secrets set "SQL:ConnectionString" "Server=192.168.1.196;Database=ffxiv;User Id=dev;Password=YourPassword;TrustServerCertificate=True;"
   ```

3. View your stored secrets (optional):
   ```bash
   dotnet user-secrets list
   ```

#### How It Works

- User secrets are stored in: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json` (Windows)
- This location is **outside your project directory** and won't be committed to git
- The secrets are automatically loaded in Development environment
- `appsettings.Development.json` is excluded via `.gitignore`

### Production Environment

For production deployments, use environment variables or a secure vault service:

#### Environment Variables

```bash
# Linux/Mac
export SQL__ConnectionString="Server=..."

# Windows PowerShell
$env:SQL__ConnectionString="Server=..."

# Windows CMD
set SQL__ConnectionString=Server=...
```
```

## Running the Application

```bash
cd ffxiv_api
dotnet restore
dotnet run
```

The API will start on `http://localhost:5071` (or check console output for the actual port).

## Project Structure

```
ffxiv_api/
├── controllers/        # API controllers
├── models/             # Data models
├── Program.cs          # Application entry point
```
