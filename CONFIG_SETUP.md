# Configuration Setup Guide

## Overview

This project uses configuration files that contain sensitive information (database credentials, API keys, JWT secrets). These files are **not tracked in git** to prevent accidental exposure of secrets.

## Setup Instructions

### 1. Create Configuration Files

Copy the template files and remove the `.template` extension:

```bash
# From the project root
cd src/CabriThonAPI.WebAPI

# Copy templates to create actual configuration files
cp appsettings.json.template appsettings.json
cp appsettings.Development.json.template appsettings.Development.json
```

### 2. Configure Your Secrets

Edit the newly created `appsettings.json` and `appsettings.Development.json` files with your actual values:

#### Database Connection String
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_SUPABASE_HOST;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
}
```

#### JWT Settings
```json
"JwtSettings": {
  "SecretKey": "YOUR_JWT_SECRET_KEY_HERE_MUST_BE_AT_LEAST_64_CHARS",
  "Issuer": "https://YOUR_SUPABASE_PROJECT.supabase.co/auth/v1",
  "Audience": "authenticated",
  "ExpirationMinutes": 60
}
```

#### Supabase Configuration
```json
"Supabase": {
  "Url": "https://YOUR_SUPABASE_PROJECT.supabase.co",
  "AnonKey": "YOUR_SUPABASE_ANON_KEY"
}
```

#### Gemini AI (Optional)
```json
"GeminiAI": {
  "ProjectId": "your-google-cloud-project-id",
  "Location": "us-central1",
  "ModelId": "gemini-1.5-flash"
}
```

### 3. Generate a Secure JWT Secret Key

You can generate a secure JWT secret key using PowerShell or OpenSSL:

**PowerShell:**
```powershell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

**OpenSSL (Linux/Mac):**
```bash
openssl rand -base64 64
```

### 4. Get Supabase Credentials

1. Log in to your [Supabase Dashboard](https://app.supabase.com/)
2. Select your project
3. Go to **Settings** → **API**
4. Copy the following:
   - **Project URL** (for `Supabase.Url`)
   - **anon/public key** (for `Supabase.AnonKey`)
5. Go to **Settings** → **Database** to get your connection string details

### 5. Verify Setup

Run the application to verify your configuration:

```bash
dotnet run --project src/CabriThonAPI.WebAPI
```

## Security Best Practices

- ✅ **Never commit** `appsettings.json` or `appsettings.Development.json` files
- ✅ Use different secrets for development and production
- ✅ Rotate secrets regularly
- ✅ Use environment variables for production deployments
- ✅ Share secrets securely with team members (use password managers, not email/chat)

## Files Ignored by Git

The following files are configured in `.gitignore`:
- `**/appsettings.json`
- `**/appsettings.Development.json`
- `**/appsettings.Production.json`

## Troubleshooting

### "Configuration is missing" error
Make sure you've created the configuration files from the templates and filled in all required values.

### Database connection fails
- Verify your Supabase connection string is correct
- Check that SSL Mode is set to "Require"
- Ensure your IP is allowed in Supabase security settings

### JWT authentication fails
- Verify the JWT secret key matches between Supabase and your configuration
- Check that the Issuer URL matches your Supabase project URL
- Ensure the secret key is at least 64 characters long

## Additional Resources

- [Supabase Documentation](https://supabase.com/docs)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

