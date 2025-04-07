# CWR Status Checker

A .NET service that monitors user accounts from multiple LDAP endpoints and manages notifications for account expiration and password changes.

## Features

- Monitors multiple LDAP endpoints for user account status
- Tracks account expiration dates and password age
- Sends notifications for:
  - Accounts nearing expiration (30 days or less)
  - Accounts that have expired
  - Passwords that need to be changed (90+ days old)
- Supports multiple LDAP domains
- Maintains history of user status in SQLite database
- Runs daily checks at 8 AM

## Prerequisites

- .NET 7.0 or later
- For production:
  - Windows Server (for LDAP connectivity)
  - SMTP server for email notifications
  - Access to LDAP/Active Directory servers
- For development:
  - Any OS (uses test services)

## Configuration

The application uses the following configuration settings in `appsettings.json`:

```json
{
  "LdapEndpoints": [
    "localhost:389",
    "test.ldap:389"
  ],
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=CwrStatusChecker.db"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.yourserver.com",
    "SmtpPort": 587,
    "Username": "your-email@domain.com",
    "Password": "your-password",
    "FromEmail": "notifications@domain.com"
  }
}
```

## Development Setup

1. Clone the repository
2. Navigate to the project directory
3. Run the application in development mode:
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project CwrStatusChecker.Service
```

In development mode:
- Uses `TestLdapService` instead of real LDAP connections
- Uses `TestEmailService` that logs emails instead of sending them
- Uses SQLite database instead of SQL Server
- Runs once and exits (instead of running continuously)

## Production Setup

1. Build the application:
```bash
dotnet publish -c Release
```

2. Configure the application settings in `appsettings.Production.json`

3. Run as a Windows Service:
```powershell
sc.exe create "CWR Status Checker" binPath="path\to\CwrStatusChecker.Service.exe"
sc.exe start "CWR Status Checker"
```

## Project Structure

- **CwrStatusChecker.Service**: Main worker service and business logic
  - `Worker.cs`: Background service that runs the checks
  - `Services/`: Service implementations (LDAP, Email)
- **CwrStatusChecker.Data**: Data access layer
  - `ApplicationDbContext.cs`: Entity Framework context
- **CwrStatusChecker.Models**: Domain models
  - `User.cs`: User account model

## Database Schema

The application uses a single `Users` table with the following schema:
- Id (int, primary key)
- Username (string, unique with LdapEndpoint)
- Email (string)
- ManagerEmail (string)
- LastPasswordChange (DateTime?)
- AccountExpirationDate (DateTime?)
- IsDisabled (bool)
- IsArchived (bool)
- LdapEndpoint (string)
- LastChecked (DateTime)

## Testing

The application includes test services for development:
- `TestLdapService`: Provides test user data
- `TestEmailService`: Logs email notifications instead of sending them

## Notifications

The service sends the following types of notifications:
1. **Account Expiration Notice**: When account expires in 30 days or less
2. **Account Disabled Notice**: When account has expired
3. **Password Change Required**: When password is older than 90 days 