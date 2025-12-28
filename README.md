# Password Manager

A secure password manager built with .NET MAUI and ASP.NET Core Web API.

## Architecture

### MAUI Application (standalone)
- **Platform:** .NET MAUI (Windows, Android, iOS, macOS)
- **Local storage:** SQLite database
- **Works offline:** Full functionality without internet connection
- **Optional cloud backup:** Connect to API for backup

### ASP.NET Core Web API (optional - cloud backup)
- **Purpose:** Cloud backup for multiple accounts
- **Database:** PostgreSQL
- **Note:** The MAUI app works independently, API is only needed for cloud features

## Features
- Local password storage (SQLite)
- Cloud backup via REST API
- Works offline - no internet required for core functionality
- Dark/Light theme support

## Platform Support
- **Windows 10/11** (x64) - tested and working

## Application (in release)
<img width="1407" height="686" alt="image" src="https://github.com/user-attachments/assets/88e66471-fecd-4998-b8c3-a02975ad0e6b" />
<img width="1413" height="675" alt="image" src="https://github.com/user-attachments/assets/dffd5ab8-6b95-4a59-91af-b44232c1c14b" />
<img width="1413" height="681" alt="image" src="https://github.com/user-attachments/assets/d96acd23-9e03-455f-81a8-8af1570b8433" />
<img width="1052" height="590" alt="image" src="https://github.com/user-attachments/assets/cfc494a2-92aa-4675-9841-20c89b9675e6" />

### Standalone application
Download: https://github.com/BreakL1256/Password-manager/releases/tag/V1.0

## Application setup (with cloud backup)
1. Clone the repository
  ```
  git clone -b main https://github.com/BreakL1256/Password-manager.git
  ```
2. Configure application (MAUI)
  - Change `Config.Example.cs` to `Config.cs`
  - Add your Syncfusion license key
  ```
  public const string SyncfusionKey = "YOUR_SYNCFUSION_LICENSE_KEY";
  ``` 
  > You can get your license key here (program will work without the key, but will throw popups): https://www.syncfusion.com/products/communitylicense

3. Configure the API
  - Change `appsettings.example.json` to `appsettings.json`
  - Add randomly generated secret key and database connection string
  ```
  "Jwt": {
      "SecretKey": "YOUR_JWT_SECRET_HERE",
      "Issuer": "YourPasswordManagerAPI",
      "Audience": "YourPasswordManagerClient",
      "ExpirationHours": 24
  },
  "ConnectionStrings": {
      "DefaultConnection": "YOUR_DATABASE_STRING_HERE"
  },
  ```
4. Run migrations
 ```
 cd Password-manager-api
 dotnet ef database update
 ```
5. Running the whole project
 - Run API
 - Run MAUI app 

> [!WARNING]
> Instructions are for setting up on visual studio 2022
