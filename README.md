[![.NET Build and Publish](https://github.com/LoGaCulture/LUTE-Server/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/LoGaCulture/LUTE-Server/actions/workflows/dotnet.yml)

# LUTE Server

LUTE Server is a backend API server designed for handling user logs, game data, and shared variables for the [LUTE Unity tool](https://github.com/LoGaCulture/LUTE).

## Features

- User authentication using JWT.
- Admin panel for viewing and filtering user logs.
- API endpoints for Unity logging and game shared variables.
- CSV download for filtered logs.
  
## Prerequisites

- .NET 5 or later
- SQLite (or any database specified in `appsettings.json`)
- Visual Studio / Visual Studio Code (or any preferred IDE)
- Optional: Docker (for containerization)

## Getting Started

### Option 1: Download the Latest Release

Instead of building the project yourself, you can download the latest release from the [Releases Page](https://github.com/LoGaCulture/LUTE-Server/releases).

1. Go to the [Releases Page](https://github.com/LoGaCulture/LUTE-Server/releases) and download the latest release `.zip` file.

2. Unzip the file and move the contents to a directory of your choice.

3. Open the `example.appsettings.json` file in the unzipped folder, and configure it by modifying the following settings and renaming it to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YourDBName.db" //Change this to the name of the database you want
  },
  "Jwt": {
    "Key": "SecretDevKeyVeryLongHasToBeForSecurity", //Change this to a secure key for JWT tokens
    "Issuer": "Your Issuer" //Change this to your organisation or anything
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",

  "DefaultAdmin": {
    "Username": "admin", // Change this username
    "Password": "admin123", // Change this password
    "Enabled": true // Set this to true to enable admin creation on first load
  }
}
```
4. Run the server:

```bash
dotnet LUTE-Server.dll
```

### Option 2: Build from Source

#### Direct Installation

1. Clone the repository:

```bash
git clone https://github.com/LoGaCulture/LUTE-Server.git
```

2. Open the project in your preferred IDE (Visual Studio, Visual Studio Code, etc.), or build it using the command line:

```bash
dotnet build
```

3. Run the server from the IDE or using the command line:

```bash
dotnet run
```

#### Docker Installation

1. Clone the repository:

```bash
git clone https://github.com/LoGaCulture/LUTE-Server.git
```

2. Run docker compose:

```bash
docker compose up
```

### Configuration

You have to configure the server by modifying the `example.appsettings.json` file and renaming it to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YourDBName.db" //Change this to the name of the database you want
  },
  "Jwt": {
    "Key": "SecretDevKeyVeryLongHasToBeForSecurity", //Change this to a secure key for JWT tokens
    "Issuer": "Your Issuer" //Change this to your organisation or anything
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",

    // Uncomment and edit the fields below to create a default admin user
  "DefaultAdmin": {
    "Username": "admin", // Change this username
    "Password": "admin123", // Change this password
    "Enabled": true // Set this to true to enable admin creation on first load
  }
}

```

To create a default admin user, please remove all the comments and change the username and password to something secure.




