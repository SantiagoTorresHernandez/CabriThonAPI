# Migración a Base de Datos Supabase (PostgreSQL)

## Resumen de Cambios Realizados

Este documento describe todos los cambios implementados para alinear el proyecto con el esquema de base de datos de Supabase (PostgreSQL).

## 1. Base de Datos - PostgreSQL (Supabase)

### Paquetes Agregados
- ✅ `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.4) - Provider de PostgreSQL para Entity Framework Core

### Cambios en Configuración
- ✅ `Program.cs` - Actualizado para usar `UseNpgsql()` en lugar de `UseSqlServer()`
- ✅ `appsettings.json` - Cadena de conexión actualizada con formato PostgreSQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-supabase-host.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

## 2. Cambios en Entidades del Domain

### IDs Cambiados de `Guid` a `long` (bigint)

| Entidad Original | Nueva Entidad | Tabla Supabase | Cambios |
|------------------|---------------|----------------|---------|
| `PromotionSuggestion` | `Promotion` | `promotion` | ✅ ID: Guid → long |
| `PromotionProduct` | `PromotionProduct` | `promotion_product` | ✅ IDs: Guid → long |
| `OrderSuggestion` | `SuggestedOrder` | `suggested_order` | ✅ ID: Guid → long |
| `OrderItem` | `SuggestedOrderItem` | `suggested_order_item` | ✅ IDs: Guid → long |
| - | `PromotionMetric` | `promotion_metrics` | ✅ Nueva entidad |
| - | `Client` | `client` | ✅ Nueva entidad |
| - | `Product` | `product` | ✅ Nueva entidad |
| - | `InventoryClient` | `inventory_client` | ✅ Nueva entidad |

### Promotion (antes PromotionSuggestion)

```csharp
public class Promotion : BaseEntity
{
    public long PromotionId { get; set; }              // bigint
    public string? PromotionCode { get; set; }
    public long ClientId { get; set; }                 // bigint FK
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? JustificationAI { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? ExpectedIncreasePercent { get; set; }
    public decimal? ProfitMarginPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Draft";      // 'Active', 'Inactive', 'Draft'
    public bool CreatedByAI { get; set; } = false;
    
    // Navigation properties
    public Client? Client { get; set; }
    public List<PromotionProduct> PromotionProducts { get; set; }
    public List<PromotionMetric> PromotionMetrics { get; set; }
}
```

### SuggestedOrder (antes OrderSuggestion)

```csharp
public class SuggestedOrder : BaseEntity
{
    public long SuggestedOrderId { get; set; }         // bigint
    public long ClientId { get; set; }                 // bigint FK
    public int Status { get; set; } = 0;               // 0=Draft, 1=Approved, 2=Rejected, 3=Applied
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Client? Client { get; set; }
    public List<SuggestedOrderItem> SuggestedOrderItems { get; set; }
}
```

## 3. Cambios en Interfaces de Repositorio

### Nuevas Interfaces

| Interfaz Original | Nueva Interfaz | Cambios |
|-------------------|----------------|---------|
| `IPromotionSuggestionRepository` | `IPromotionRepository` | ✅ Parámetros: string → long |
| `IOrderSuggestionRepository` | `ISuggestedOrderRepository` | ✅ Parámetros: string → long, SuggestionStatus? → int? |
| `IImpactMetricRepository` | `IImpactMetricRepository` | ✅ Parámetros: string → long |

## 4. Cambios en ApplicationDbContext

### Mapeo a Tablas de Supabase

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Schema público de Supabase
    modelBuilder.HasDefaultSchema("public");

    // Mapeo de nombres de columnas a snake_case
    entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
    entity.Property(e => e.ClientId).HasColumnName("client_id");
    // ... etc
}
```

### Tablas Mapeadas

- ✅ `client` - Clientes/Tiendas
- ✅ `product` - Productos
- ✅ `promotion` - Promociones
- ✅ `promotion_product` - Productos en promociones
- ✅ `promotion_metrics` - Métricas de promociones
- ✅ `suggested_order` - Órdenes sugeridas
- ✅ `suggested_order_item` - Items de órdenes sugeridas
- ✅ `inventory_client` - Inventario por cliente

## 5. Cambios en Repositorios

### Nuevos Repositorios

| Repositorio Original | Nuevo Repositorio | Archivo |
|---------------------|-------------------|---------|
| `PromotionSuggestionRepository` | `PromotionRepository` | `PromotionRepository.cs` |
| `OrderSuggestionRepository` | `SuggestedOrderRepository` | `SuggestedOrderRepository.cs` |
| `ImpactMetricRepository` | `ImpactMetricRepository` | `ImpactMetricRepository.cs` (actualizado) |

### Cambios Clave

- ✅ Queries actualizadas para usar `long` en lugar de `Guid`
- ✅ Includes actualizados para cargar relaciones con `Client` y `Product`
- ✅ Métrica de impacto calculada dinámicamente desde `promotion_metrics` y `suggested_orders`

## 6. Cambios en DTOs

### Tipos de Datos Actualizados

```csharp
// Antes
public class PromotionSuggestionDto
{
    public string PromotionId { get; set; }  // Guid como string
    public string ProductId { get; set; }
}

// Después
public class PromotionSuggestionDto
{
    public long PromotionId { get; set; }    // long (bigint)
    public long ProductId { get; set; }
}
```

- ✅ `PromotionSuggestionDto` - IDs actualizados a `long`
- ✅ `OrderSuggestionDto` - IDs actualizados a `long`
- ✅ `ProductDto` - IDs actualizados a `long`
- ✅ `StockDto` - IDs actualizados a `long`
- ✅ `OrderHistoryDto` - IDs actualizados a `long`

## 7. Cambios en Servicios

### SuggestionService

```csharp
// Conversión de clientId string a long
if (!long.TryParse(clientId, out var clientIdLong))
{
    throw new ArgumentException("Invalid clientId format", nameof(clientId));
}

// Mapeo de status para SuggestedOrder (string → int)
statusInt = status.ToLower() switch
{
    "draft" => 0,
    "approved" => 1,
    "rejected" => 2,
    "applied" => 3,
    _ => null
};
```

### MetricsService

- ✅ Conversión de `clientId` de string a long
- ✅ Cálculo de métricas desde tablas reales de Supabase

### AIAgentService

- ✅ Usa nuevas entidades `Promotion` y `SuggestedOrder`
- ✅ Conversión de `clientId` a long
- ✅ Creación de relaciones con `PromotionProduct` y `SuggestedOrderItem`

## 8. Cambios en Program.cs

### Registro de Repositorios Actualizado

```csharp
// Antes
builder.Services.AddScoped<IPromotionSuggestionRepository, PromotionSuggestionRepository>();
builder.Services.AddScoped<IOrderSuggestionRepository, OrderSuggestionRepository>();

// Después
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<ISuggestedOrderRepository, SuggestedOrderRepository>();
```

## 9. Estado de Mapeo de Tablas

| Tabla Supabase | Entidad .NET | Estado | Uso |
|----------------|--------------|--------|-----|
| `client` | `Client` | ✅ Mapeada | Relación FK |
| `product` | `Product` | ✅ Mapeada | Relación FK |
| `promotion` | `Promotion` | ✅ Mapeada | Endpoint `/suggestions/promotions` |
| `promotion_product` | `PromotionProduct` | ✅ Mapeada | Relación N:M |
| `promotion_metrics` | `PromotionMetric` | ✅ Mapeada | Endpoint `/metrics/impact/promotions` |
| `suggested_order` | `SuggestedOrder` | ✅ Mapeada | Endpoint `/suggestions/orders` |
| `suggested_order_item` | `SuggestedOrderItem` | ✅ Mapeada | Relación N:M |
| `inventory_client` | `InventoryClient` | ✅ Mapeada | Para consultas de stock |
| `orders` | - | ⚠️ No mapeada | Podría usarse para historial |
| `order_item` | - | ⚠️ No mapeada | Podría usarse para historial |
| `predicciones_ventas` | - | ⚠️ No mapeada | Podría usarse para IA |
| `alertas_inventario` | - | ⚠️ No mapeada | Podría usarse para alertas |

## 10. Consideraciones de Uso con clientId

### En el Token JWT

El middleware extrae el `clientId` del token JWT. El claim debe ser:

```json
{
  "clientId": "1",        // String que representa un long
  "sub": "store-1",
  "email": "store1@example.com"
}
```

### En la API

- El `clientId` se pasa como **string** desde el token
- Se convierte a **long** en los servicios: `long.TryParse(clientId, out var clientIdLong)`
- Se usa como **long** en las consultas a la base de datos

## 11. Próximos Pasos

### Para Usar con Supabase

1. **Actualizar cadena de conexión** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=db.{your-project}.supabase.co;Port=5432;Database=postgres;Username=postgres;Password={your-password};SSL Mode=Require;Trust Server Certificate=true"
     }
   }
   ```

2. **NO ejecutar migraciones** - Las tablas ya existen en Supabase

3. **Configurar autenticación**: El sistema ya usa las tablas existentes

4. **Poblar datos iniciales** si es necesario (clientes, productos)

### Para Desarrollo Local

El proyecto sigue usando In-Memory Database por defecto cuando no hay cadena de conexión configurada.

## 12. Arquitectura Final

```
Controllers (WebAPI)
    ↓
Services (Application)
    ↓ (conversión string → long)
Repositories (Infrastructure)
    ↓
DbContext (PostgreSQL/Supabase)
    ↓
Tablas Supabase
```

## 13. Beneficios de la Migración

✅ **Compatibilidad Total** con esquema existente de Supabase
✅ **Sin Conflictos** de IDs (bigint en lugar de GUID)
✅ **Relaciones Reales** con Client y Product
✅ **Métricas Reales** calculadas desde promotion_metrics
✅ **Escalabilidad** con PostgreSQL empresarial
✅ **Multitenancy** natural con clientId como FK

## 14. Compilación

El proyecto compila exitosamente con estos cambios. Para compilar:

```bash
# Detener cualquier instancia corriendo
taskkill /F /IM CabriThonAPI.WebAPI.exe

# Compilar
dotnet build

# Ejecutar
dotnet run --project src/CabriThonAPI.WebAPI
```

---

**Fecha de Migración**: 2025-11-04
**Estado**: ✅ Completado
**Compatibilidad**: PostgreSQL 14+ / Supabase

