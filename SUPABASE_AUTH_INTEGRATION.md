# Supabase Authentication Integration Guide

## Overview

This API is configured to work with **Supabase Authentication**. The frontend authenticates users via Supabase, and this backend API validates the JWT tokens from Supabase.

---

## 🔧 Backend Setup (This Project)

### Step 1: Get Supabase Credentials

1. Go to [Supabase Dashboard](https://supabase.com/dashboard)
2. Select your project: `dkhluiutbrzzbwfrkveo`
3. Navigate to **Settings** → **API**
4. Copy these values:
   - **Project URL**: `https://dkhluiutbrzzbwfrkveo.supabase.co`
   - **Anon/Public Key**: `eyJhbGc...` (starts with eyJ)
   - **JWT Secret**: (scroll down to "JWT Settings" section)

### Step 2: Update `appsettings.json`

Replace the placeholders in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "PASTE_YOUR_JWT_SECRET_HERE",
    "Issuer": "https://dkhluiutbrzzbwfrkveo.supabase.co/auth/v1",
    "Audience": "authenticated",
    "ExpirationMinutes": 60
  },
  "Supabase": {
    "Url": "https://dkhluiutbrzzbwfrkveo.supabase.co",
    "AnonKey": "PASTE_YOUR_ANON_KEY_HERE"
  }
}
```

### Step 3: Database Schema - User to Client Mapping

You need to map Supabase users to your `client` table. Choose one approach:

#### **Option A: Store Supabase User ID in Client Table**

Add a column to your `client` table:

```sql
ALTER TABLE client 
ADD COLUMN auth_user_id UUID UNIQUE,
ADD CONSTRAINT fk_auth_user FOREIGN KEY (auth_user_id) REFERENCES auth.users(id);
```

Then when a user signs up in frontend:
1. Create user in Supabase Auth
2. Create corresponding client record with the `auth_user_id`

#### **Option B: Use Custom Claims in Supabase JWT**

Add `clientId` to the JWT token via Supabase database trigger:

```sql
-- In Supabase SQL Editor
CREATE OR REPLACE FUNCTION public.custom_access_token_hook(event jsonb)
RETURNS jsonb
LANGUAGE plpgsql
AS $$
DECLARE
  claims jsonb;
  client_record record;
BEGIN
  -- Get client_id for this user
  SELECT client_id INTO client_record
  FROM public.client
  WHERE auth_user_id = (event->>'user_id')::uuid;
  
  -- Add client_id to claims
  claims := event->'claims';
  
  IF client_record IS NOT NULL THEN
    claims := jsonb_set(claims, '{clientId}', to_jsonb(client_record.client_id::text));
  END IF;
  
  -- Update event
  event := jsonb_set(event, '{claims}', claims);
  
  RETURN event;
END;
$$;

-- Enable the hook
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
```

### Step 4: Restart the API

```bash
dotnet run --project src/CabriThonAPI.WebAPI
```

---

## 🌐 Frontend Setup (Separate Repository)

### Step 1: Install Supabase Client

```bash
npm install @supabase/supabase-js
# or
yarn add @supabase/supabase-js
```

### Step 2: Initialize Supabase Client

```typescript
// src/lib/supabase.ts
import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://dkhluiutbrzzbwfrkveo.supabase.co'
const supabaseAnonKey = 'YOUR_SUPABASE_ANON_KEY_HERE'

export const supabase = createClient(supabaseUrl, supabaseAnonKey)
```

### Step 3: Implement Authentication

#### Sign Up
```typescript
// src/services/auth.service.ts
import { supabase } from '../lib/supabase'

export async function signUp(email: string, password: string, storeName: string) {
  // 1. Create user in Supabase Auth
  const { data: authData, error: authError } = await supabase.auth.signUp({
    email,
    password,
  })

  if (authError) throw authError

  // 2. Create client record in your backend
  const token = authData.session?.access_token
  
  if (token) {
    await fetch('https://your-api.com/api/v1/clients', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: storeName,
        email: email,
        authUserId: authData.user?.id
      })
    })
  }

  return authData
}
```

#### Sign In
```typescript
export async function signIn(email: string, password: string) {
  const { data, error } = await supabase.auth.signInWithPassword({
    email,
    password,
  })

  if (error) throw error

  // Store token for API calls
  localStorage.setItem('supabase_token', data.session.access_token)
  
  return data
}
```

#### Sign Out
```typescript
export async function signOut() {
  const { error } = await supabase.auth.signOut()
  if (error) throw error
  
  localStorage.removeItem('supabase_token')
}
```

### Step 4: Make API Calls with JWT Token

```typescript
// src/services/api.service.ts
import { supabase } from '../lib/supabase'

const API_BASE_URL = 'http://localhost:5272/api/v1'

async function getAuthHeaders() {
  // Get current session
  const { data: { session } } = await supabase.auth.getSession()
  
  if (!session?.access_token) {
    throw new Error('No active session')
  }

  return {
    'Authorization': `Bearer ${session.access_token}`,
    'Content-Type': 'application/json',
  }
}

// Example: Get promotions
export async function getPromotions() {
  const headers = await getAuthHeaders()
  
  const response = await fetch(`${API_BASE_URL}/suggestions/promotions`, {
    headers
  })

  if (!response.ok) {
    throw new Error(`API error: ${response.statusText}`)
  }

  return response.json()
}

// Example: Get metrics
export async function getOrderMetrics(year: number) {
  const headers = await getAuthHeaders()
  
  const response = await fetch(
    `${API_BASE_URL}/metrics/impact/suggested-orders?year=${year}`,
    { headers }
  )

  if (!response.ok) {
    throw new Error(`API error: ${response.statusText}`)
  }

  return response.json()
}
```

### Step 5: Protected Routes (React Example)

```typescript
// src/components/ProtectedRoute.tsx
import { useEffect, useState } from 'react'
import { supabase } from '../lib/supabase'
import { Navigate } from 'react-router-dom'

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session)
      setLoading(false)
    })

    const {
      data: { subscription },
    } = supabase.auth.onAuthStateChange((_event, session) => {
      setSession(session)
    })

    return () => subscription.unsubscribe()
  }, [])

  if (loading) {
    return <div>Loading...</div>
  }

  if (!session) {
    return <Navigate to="/login" />
  }

  return <>{children}</>
}
```

---

## 🔐 JWT Token Structure

### Supabase Token Claims

When frontend authenticates, Supabase generates a JWT with these claims:

```json
{
  "aud": "authenticated",
  "exp": 1234567890,
  "sub": "user-uuid-from-supabase",
  "email": "user@example.com",
  "phone": "",
  "app_metadata": {
    "provider": "email",
    "providers": ["email"]
  },
  "user_metadata": {
    "email": "user@example.com"
  },
  "role": "authenticated",
  "aal": "aal1",
  "amr": [
    {
      "method": "password",
      "timestamp": 1234567890
    }
  ],
  "session_id": "session-uuid",
  "iss": "https://dkhluiutbrzzbwfrkveo.supabase.co/auth/v1"
}
```

### Custom Claims (If Using Option B)

If you add custom claims, the token will also include:

```json
{
  "clientId": "1"
}
```

---

## 🧪 Testing

### Test with cURL

```bash
# 1. Get token from Supabase (use frontend or Supabase dashboard)
TOKEN="eyJhbGc..."

# 2. Test API endpoint
curl -X GET "http://localhost:5272/api/v1/suggestions/promotions" \
  -H "Authorization: Bearer $TOKEN"
```

### Test with Postman

1. **Login via Supabase** (in your frontend)
2. **Copy the JWT token** from localStorage or browser
3. In Postman:
   - Method: `GET`
   - URL: `http://localhost:5272/api/v1/suggestions/promotions`
   - Headers: `Authorization: Bearer YOUR_TOKEN_HERE`
   - Click **Send**

---

## 🚨 Troubleshooting

### "401 Unauthorized"

**Cause**: Token is invalid or expired

**Solutions**:
- Check that JWT Secret in `appsettings.json` matches Supabase
- Verify token hasn't expired (Supabase tokens expire after 1 hour by default)
- Check Issuer and Audience match Supabase settings

### "ClientId not found in token"

**Cause**: Token doesn't have `clientId` claim and user not mapped to client

**Solutions**:
- Implement Option A or B from "User to Client Mapping" section
- Ensure `auth_user_id` is set in `client` table

### CORS Errors

**Cause**: Frontend domain not allowed

**Solution**: Update CORS policy in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://your-frontend-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Use the policy
app.UseCors("AllowFrontend");
```

---

## 📚 Additional Resources

- [Supabase Auth Documentation](https://supabase.com/docs/guides/auth)
- [JWT.io - Decode JWT tokens](https://jwt.io/)
- [Supabase JavaScript Client](https://supabase.com/docs/reference/javascript/auth-signup)

---

## 🔄 Authentication Flow Diagram

```
┌─────────────┐
│   Frontend  │
│   (Login)   │
└──────┬──────┘
       │
       │ 1. signInWithPassword()
       ▼
┌─────────────┐
│  Supabase   │
│    Auth     │
└──────┬──────┘
       │
       │ 2. Returns JWT Token
       ▼
┌─────────────┐
│   Frontend  │
│ (Store JWT) │
└──────┬──────┘
       │
       │ 3. API Request with
       │    Authorization: Bearer <token>
       ▼
┌─────────────┐
│  Backend    │
│     API     │
└──────┬──────┘
       │
       │ 4. Validates JWT
       │    Extracts clientId
       ▼
┌─────────────┐
│  Supabase   │
│  Database   │
└─────────────┘
```

---

**Last Updated**: November 4, 2025
**Supabase Project**: `dkhluiutbrzzbwfrkveo`

