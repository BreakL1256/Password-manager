# Password Manager

A secure password manager built with .NET MAUI and ASP.NET Core Web API.

## Architecture

### MAUI Application (Standalone)
- **Platform:** .NET MAUI (Windows, Android, iOS, macOS)
- **Local Storage:** SQLite database
- **Works offline:** Full functionality without internet connection
- **Optional Cloud Sync:** Connect to API for backup

### ASP.NET Core Web API (Optional - Cloud Backup)
- **Purpose:** Cloud backup for multiple accounts
- **Database:** PostgreSQL
- **Authentication:** JWT-based
- **Note:** The MAUI app works independently; API is only needed for cloud features

## Features
- Local password storage (SQLite)
- Cloud backup via REST API
- Works offline - no internet required for core functionality
- Dark/Light theme support

## Platform Support
- **Windows 10/11** (x64) - tested and working

