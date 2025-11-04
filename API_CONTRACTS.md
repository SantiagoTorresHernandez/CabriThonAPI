# API Contracts - CabriThon AI Agents API

Este documento especifica los contratos JSON de todos los endpoints de la API.

## Base URL

```
Production: https://api.cabrithon.com/api/v1
Development: http://localhost:5000/api/v1
```

## Autenticación

Todos los endpoints requieren autenticación mediante JWT Bearer Token:

```http
Authorization: Bearer <token>
```

O mediante API Key (desarrollo):

```http
X-API-Key: <api-key>
```

El token debe contener un claim `clientId` que identifica al cliente/tienda.

---

## 1. GET /suggestions/promotions

Obtiene las sugerencias de promociones para el cliente autenticado.

### Query Parameters

| Parámetro | Tipo     | Requerido | Descripción                                      |
|-----------|----------|-----------|--------------------------------------------------|
| status    | string   | No        | Filtrar por estado: Draft, Approved, Rejected, Applied |
| limit     | integer  | No        | Número máximo de sugerencias a retornar          |

### Request Example

```http
GET /api/v1/suggestions/promotions?status=Draft&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response 200 OK

```json
{
  "suggestions": [
    {
      "promotionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "Draft",
      "justificationAI": "Based on inventory analysis, combining low-rotation product 'Winter Jacket' with high-rotation 'Thermal Gloves' can boost sales by creating an attractive bundle for cold weather. Historical data shows customers who buy jackets also frequently purchase gloves.",
      "expectedIncreasePercent": 15.5,
      "products": [
        {
          "productId": "PROD001",
          "productName": "Winter Jacket",
          "discountPercent": 20.0,
          "role": "primary"
        },
        {
          "productId": "PROD045",
          "productName": "Thermal Gloves",
          "discountPercent": 15.0,
          "role": "combo"
        }
      ],
      "createdAt": "2025-11-04T10:30:00Z"
    }
  ],
  "count": 1
}
```

### Response 401 Unauthorized

```json
{
  "error": "Invalid or missing authentication token"
}
```

### Response 500 Internal Server Error

```json
{
  "error": "An error occurred while processing your request"
}
```

---

## 2. GET /suggestions/orders

Obtiene las sugerencias de pedidos de reabastecimiento para el cliente autenticado.

### Query Parameters

| Parámetro | Tipo     | Requerido | Descripción                                      |
|-----------|----------|-----------|--------------------------------------------------|
| status    | string   | No        | Filtrar por estado: Draft, Approved, Rejected, Applied |

### Request Example

```http
GET /api/v1/suggestions/orders?status=Draft
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response 200 OK

```json
{
  "suggestions": [
    {
      "suggestedOrderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "status": "Draft",
      "totalEstimatedCost": 1250.00,
      "items": [
        {
          "productId": "PROD023",
          "productName": "USB-C Cable 2m",
          "suggestedQuantity": 50,
          "currentStock": 12,
          "reorderPoint": 30,
          "unitCost": 5.00,
          "justification": "Stock below reorder point. Current: 12 units, Reorder Point: 30 units. Average daily sales: 3 units. Lead time: 5 days. Recommended order quantity accounts for lead time and safety stock."
        },
        {
          "productId": "PROD067",
          "productName": "Wireless Mouse",
          "suggestedQuantity": 40,
          "currentStock": 8,
          "reorderPoint": 25,
          "unitCost": 12.50,
          "justification": "Critical stock level. Current: 8 units, Reorder Point: 25 units. High demand product with consistent sales velocity."
        }
      ],
      "createdAt": "2025-11-04T09:15:00Z"
    }
  ],
  "count": 1
}
```

### Response Codes

- `200 OK`: Solicitud exitosa
- `401 Unauthorized`: Token inválido o faltante
- `500 Internal Server Error`: Error del servidor

---

## 3. GET /metrics/impact/suggested-orders

Obtiene las métricas de impacto de negocio por pedidos sugeridos (prevención de quiebre de stock).

### Query Parameters

| Parámetro | Tipo     | Requerido | Descripción                    |
|-----------|----------|-----------|--------------------------------|
| year      | integer  | No        | Año para métricas (default: año actual) |

### Request Example

```http
GET /api/v1/metrics/impact/suggested-orders?year=2025
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response 200 OK

```json
{
  "year": 2025,
  "totalBenefit": 45000.00,
  "monthlyBreakdown": [
    {
      "month": 1,
      "benefitAmount": 3500.00,
      "itemsCount": 15
    },
    {
      "month": 2,
      "benefitAmount": 4200.00,
      "itemsCount": 18
    },
    {
      "month": 3,
      "benefitAmount": 3800.00,
      "itemsCount": 16
    },
    {
      "month": 4,
      "benefitAmount": 4100.00,
      "itemsCount": 17
    },
    {
      "month": 5,
      "benefitAmount": 3900.00,
      "itemsCount": 14
    },
    {
      "month": 6,
      "benefitAmount": 4300.00,
      "itemsCount": 19
    },
    {
      "month": 7,
      "benefitAmount": 3700.00,
      "itemsCount": 13
    },
    {
      "month": 8,
      "benefitAmount": 4000.00,
      "itemsCount": 15
    },
    {
      "month": 9,
      "benefitAmount": 3600.00,
      "itemsCount": 12
    },
    {
      "month": 10,
      "benefitAmount": 4500.00,
      "itemsCount": 20
    },
    {
      "month": 11,
      "benefitAmount": 4200.00,
      "itemsCount": 18
    },
    {
      "month": 12,
      "benefitAmount": 5200.00,
      "itemsCount": 22
    }
  ]
}
```

### Campo Descriptions

- `year`: Año de las métricas
- `totalBenefit`: Beneficio total estimado por prevención de quiebres de stock (pérdidas evitadas)
- `monthlyBreakdown`: Desglose mensual
  - `month`: Número del mes (1-12)
  - `benefitAmount`: Beneficio del mes (ventas que se habrían perdido)
  - `itemsCount`: Cantidad de pedidos sugeridos aplicados en el mes

### Response Codes

- `200 OK`: Solicitud exitosa
- `401 Unauthorized`: Token inválido o faltante
- `500 Internal Server Error`: Error del servidor

---

## 4. GET /metrics/impact/promotions

Obtiene las métricas de impacto de negocio por promociones sugeridas (ventas incrementales).

### Query Parameters

| Parámetro | Tipo     | Requerido | Descripción                    |
|-----------|----------|-----------|--------------------------------|
| year      | integer  | No        | Año para métricas (default: año actual) |

### Request Example

```http
GET /api/v1/metrics/impact/promotions?year=2025
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response 200 OK

```json
{
  "year": 2025,
  "totalBenefit": 78000.00,
  "monthlyBreakdown": [
    {
      "month": 1,
      "benefitAmount": 5500.00,
      "itemsCount": 8
    },
    {
      "month": 2,
      "benefitAmount": 6200.00,
      "itemsCount": 9
    },
    {
      "month": 3,
      "benefitAmount": 6800.00,
      "itemsCount": 10
    },
    {
      "month": 4,
      "benefitAmount": 7100.00,
      "itemsCount": 11
    },
    {
      "month": 5,
      "benefitAmount": 6900.00,
      "itemsCount": 10
    },
    {
      "month": 6,
      "benefitAmount": 7300.00,
      "itemsCount": 12
    },
    {
      "month": 7,
      "benefitAmount": 5700.00,
      "itemsCount": 7
    },
    {
      "month": 8,
      "benefitAmount": 6000.00,
      "itemsCount": 8
    },
    {
      "month": 9,
      "benefitAmount": 6600.00,
      "itemsCount": 9
    },
    {
      "month": 10,
      "benefitAmount": 7500.00,
      "itemsCount": 13
    },
    {
      "month": 11,
      "benefitAmount": 6200.00,
      "itemsCount": 9
    },
    {
      "month": 12,
      "benefitAmount": 8200.00,
      "itemsCount": 15
    }
  ]
}
```

### Campo Descriptions

- `year`: Año de las métricas
- `totalBenefit`: Beneficio total por ventas incrementales debido a promociones
- `monthlyBreakdown`: Desglose mensual
  - `month`: Número del mes (1-12)
  - `benefitAmount`: Ventas incrementales del mes
  - `itemsCount`: Cantidad de promociones aplicadas en el mes

### Response Codes

- `200 OK`: Solicitud exitosa
- `401 Unauthorized`: Token inválido o faltante
- `500 Internal Server Error`: Error del servidor

---

## Error Responses

Todos los endpoints pueden retornar los siguientes errores:

### 400 Bad Request

```json
{
  "error": "Invalid parameter value",
  "details": {
    "parameter": "status",
    "message": "Status must be one of: Draft, Approved, Rejected, Applied"
  }
}
```

### 401 Unauthorized

```json
{
  "error": "Invalid or missing authentication token"
}
```

### 403 Forbidden

```json
{
  "error": "Access denied to the requested resource"
}
```

### 404 Not Found

```json
{
  "error": "Resource not found"
}
```

### 500 Internal Server Error

```json
{
  "error": "An error occurred while processing your request"
}
```

---

## Rate Limiting

**Not implemented yet**. Future implementation will include:

- 100 requests per minute per client
- Header `X-RateLimit-Remaining` indica requests restantes
- Status `429 Too Many Requests` cuando se excede el límite

---

## Versioning

La API usa versionado en la URL: `/api/v1/...`

Cambios breaking introducirán una nueva versión (`/api/v2/...`) mientras se mantiene compatibilidad con versiones anteriores por un período de transición.

---

## Notes

1. Todos los timestamps están en formato ISO 8601 UTC
2. Todos los montos monetarios son decimales con 2 decimales
3. Los IDs son UUIDs en formato string
4. Las respuestas usan camelCase para los nombres de campos
5. Los códigos de estado HTTP siguen las convenciones REST estándar

