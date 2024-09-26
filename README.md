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

### Installation

You can either run it directly on your machine or use Docker for containerization.

#### Direct Installation

1. Clone the repository:

```bash
git clone https://github.com/LoGaCulture/LUTE-Server.git
```

2. Open the project in your preferred IDE.

3. Run the project.

#### Docker Installation

Run docker-compose:

```bash 
docker-compose up
```

### Configuration

You have to configure the server by modifying the `example.appsettings.json` file and renaming it to appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YourDBName.db"
  },
  "Jwt": {
    "Key": "SecretDevKeyVeryLongHasToBeForSecurity",
    "Issuer": "Your Issuer"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",

    /* Uncomment and edit the fields below to create a default admin user
  "DefaultAdmin": {
    "Username": "admin", // Change this username
    "Password": "admin123", // Change this password
    "Enabled": true // Set this to true to enable admin creation
  }*/
}

```

To create a default admin user, please remove all the comments and change the username and password to something secure.


