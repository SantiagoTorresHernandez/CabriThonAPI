# Quick Start Guide - CabriThon AI Agents API

Esta guía te ayudará a poner en marcha el proyecto rápidamente.

## Requisitos Previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Editor de código (Visual Studio 2022, Visual Studio Code, Rider)
- (Opcional) SQL Server para base de datos persistente
- (Opcional) Cuenta de Google Cloud con Vertex AI habilitado

## Instalación Rápida

### 1. Clonar el Repositorio

```bash
git clone <repository-url>
cd CabriThonAPI
```

### 2. Restaurar Dependencias

```bash
dotnet restore
```

### 3. Compilar el Proyecto

```bash
dotnet build
```

### 4. Ejecutar la API

```bash
dotnet run --project src/CabriThonAPI.WebAPI
```

La API estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001`

## Configuración Rápida

### Desarrollo Local (Sin Configuración)

Por defecto, el proyecto usa:
- **Base de datos en memoria** (no requiere SQL Server)
- **Mock de Gemini AI** (respuestas de desarrollo)
- **Autenticación relajada** (acepta cualquier API Key)

Puedes empezar a usar la API inmediatamente sin configuración adicional.

### Probar la API con Swagger

1. Navega a `https://localhost:5001`
2. Click en "Authorize"
3. Ingresa cualquier API Key (ej: `test-store-123`)
4. Prueba los endpoints directamente desde la interfaz

### Endpoints Disponibles

```http
GET /api/v1/suggestions/promotions
GET /api/v1/suggestions/orders
GET /api/v1/metrics/impact/suggested-orders
GET /api/v1/metrics/impact/promotions
```

## Autenticación de Desarrollo

### Opción 1: API Key (Más Simple)

```bash
curl -H "X-API-Key: test-store-123" https://localhost:5001/api/v1/suggestions/promotions
```

### Opción 2: JWT Token

Para usar JWT, necesitas generar un token. Puedes usar [jwt.io](https://jwt.io):

**Payload:**
```json
{
  "sub": "store-123",
  "clientId": "store-123",
  "iat": 1699012345,
  "exp": 1999098745
}
```

**Secret:** `YourSuperSecretKeyForDevelopmentPurposes123456789`

**Request:**
```bash
curl -H "Authorization: Bearer <tu-token>" https://localhost:5001/api/v1/suggestions/promotions
```

## Configuración Avanzada

### Usar SQL Server

1. Actualiza `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CabriThonDB;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

2. Ejecuta migraciones:

```bash
dotnet ef migrations add InitialCreate --project src/CabriThonAPI.Infrastructure --startup-project src/CabriThonAPI.WebAPI
dotnet ef database update --project src/CabriThonAPI.Infrastructure --startup-project src/CabriThonAPI.WebAPI
```

### Configurar Gemini AI

1. Crea un proyecto en [Google Cloud Console](https://console.cloud.google.com)
2. Habilita Vertex AI API
3. Configura credenciales:

```bash
# Linux/Mac
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/credentials.json"

# Windows PowerShell
$env:GOOGLE_APPLICATION_CREDENTIALS="C:\path\to\credentials.json"
```

4. Actualiza `appsettings.Development.json`:

```json
{
  "GeminiAI": {
    "ProjectId": "tu-proyecto-id",
    "Location": "us-central1",
    "ModelId": "gemini-1.5-flash"
  }
}
```

## Estructura del Proyecto

```
CabriThonAPI/
├── src/
│   ├── CabriThonAPI.Domain/          # Entidades de negocio
│   ├── CabriThonAPI.Application/     # Lógica de aplicación
│   ├── CabriThonAPI.Infrastructure/  # Acceso a datos e integraciones
│   └── CabriThonAPI.WebAPI/          # API REST
├── README.md                          # Documentación completa
├── ARCHITECTURE.md                    # Arquitectura detallada
├── API_CONTRACTS.md                   # Contratos de la API
└── QUICK_START.md                     # Esta guía
```

## Comandos Útiles

### Desarrollo

```bash
# Watch mode (recarga automática)
dotnet watch --project src/CabriThonAPI.WebAPI

# Ver logs detallados
dotnet run --project src/CabriThonAPI.WebAPI --verbosity detailed
```

### Testing

```bash
# Ejecutar tests (cuando los agregues)
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true
```

### Base de Datos

```bash
# Crear migración
dotnet ef migrations add <NombreMigracion> --project src/CabriThonAPI.Infrastructure --startup-project src/CabriThonAPI.WebAPI

# Aplicar migraciones
dotnet ef database update --project src/CabriThonAPI.Infrastructure --startup-project src/CabriThonAPI.WebAPI

# Eliminar última migración
dotnet ef migrations remove --project src/CabriThonAPI.Infrastructure --startup-project src/CabriThonAPI.WebAPI
```

## Ejemplo de Uso Completo

### 1. Obtener Promociones

```bash
curl -X GET "https://localhost:5001/api/v1/suggestions/promotions?status=Draft&limit=5" \
     -H "X-API-Key: store-123" \
     -k
```

**Respuesta:**
```json
{
  "suggestions": [
    {
      "promotionId": "guid",
      "status": "Draft",
      "justificationAI": "...",
      "expectedIncreasePercent": 15.5,
      "products": [...],
      "createdAt": "2025-11-04T10:00:00Z"
    }
  ],
  "count": 1
}
```

### 2. Obtener Órdenes Sugeridas

```bash
curl -X GET "https://localhost:5001/api/v1/suggestions/orders" \
     -H "X-API-Key: store-123" \
     -k
```

### 3. Obtener Métricas de Impacto

```bash
curl -X GET "https://localhost:5001/api/v1/metrics/impact/promotions?year=2025" \
     -H "X-API-Key: store-123" \
     -k
```

## Datos de Prueba

El proyecto en modo desarrollo con base de datos en memoria inicia vacío. Los agentes de IA generarán datos cuando se ejecuten.

Para ejecutar manualmente los agentes (futuro):
- Endpoint de Agente de Promociones (TBD)
- Endpoint de Agente de Reabastecimiento (TBD)

## Solución de Problemas

### Error: "Unable to bind to https://localhost:5001"

**Solución**: El puerto está en uso. Cambia el puerto en `Properties/launchSettings.json`

### Error: "No connection string configured"

**Solución**: Está bien, el proyecto usará la base de datos en memoria automáticamente.

### Error de certificado SSL en desarrollo

**Solución**: Confía en el certificado de desarrollo de .NET:

```bash
dotnet dev-certs https --trust
```

### Swagger no carga

**Solución**: Asegúrate de estar accediendo a `https://localhost:5001` (no HTTP)

## Próximos Pasos

1. **Lee la documentación completa**: `README.md`
2. **Entiende la arquitectura**: `ARCHITECTURE.md`
3. **Revisa los contratos de API**: `API_CONTRACTS.md`
4. **Configura autenticación real** para producción
5. **Agrega tests unitarios e integración**
6. **Configura CI/CD** para deployment automático

## Recursos Adicionales

- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Vertex AI Documentation](https://cloud.google.com/vertex-ai/docs)
- [JWT.io](https://jwt.io) - Debug JWT tokens
- [Swagger/OpenAPI](https://swagger.io/)

## Soporte

Para preguntas o problemas:
1. Revisa la documentación completa
2. Busca en los issues del repositorio
3. Crea un nuevo issue con detalles del problema

---

¡Disfruta desarrollando con CabriThon AI Agents API! 🚀

