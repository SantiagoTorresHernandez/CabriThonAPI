# Arquitectura del Sistema - CabriThon AI Agents API

## Visión General

Este documento describe la arquitectura del backend de AI Agents & Suggestions, diseñado siguiendo los principios de **Clean Architecture**.

## Principios de Clean Architecture

1. **Independencia de frameworks**: La lógica de negocio no depende de frameworks externos
2. **Testeable**: La lógica de negocio puede testearse sin UI, base de datos o servicios externos
3. **Independiente de la UI**: La UI puede cambiar sin afectar la lógica
4. **Independiente de la base de datos**: La lógica no está atada a un proveedor específico
5. **Independiente de servicios externos**: La lógica no conoce detalles de servicios externos

## Capas de la Arquitectura

### 1. Domain Layer (Núcleo)

**Responsabilidad**: Contiene las entidades de negocio y las reglas de negocio empresariales.

**Componentes**:
- `Entities/`: Objetos de negocio (PromotionSuggestion, OrderSuggestion, ImpactMetric)
- `Interfaces/`: Contratos de repositorio

**Características**:
- No tiene dependencias externas
- Define interfaces que serán implementadas en capas externas
- Contiene enumeraciones y tipos de valor

### 2. Application Layer

**Responsabilidad**: Contiene las reglas de negocio de la aplicación y orquesta el flujo de datos.

**Componentes**:
- `DTOs/`: Data Transfer Objects para comunicación con el exterior
- `Interfaces/`: Contratos de servicios de aplicación
- `Services/`: Implementación de lógica de aplicación

**Características**:
- Depende solo de la capa Domain
- Define interfaces para servicios de infraestructura
- Coordina la ejecución de casos de uso

### 3. Infrastructure Layer

**Responsabilidad**: Implementa interfaces definidas en capas internas y provee acceso a recursos externos.

**Componentes**:
- `Data/`: DbContext, configuración de Entity Framework
- `Repositories/`: Implementaciones de repositorios
- `Services/`: Servicios externos (Gemini AI, HTTP clients)

**Características**:
- Implementa interfaces de Domain y Application
- Maneja detalles de infraestructura (DB, APIs externas, AI)
- Puede ser reemplazada sin afectar la lógica de negocio

### 4. WebAPI Layer (Presentación)

**Responsabilidad**: Punto de entrada de la aplicación, maneja HTTP y autenticación.

**Componentes**:
- `Controllers/`: Endpoints de la API REST
- `Middleware/`: Procesamiento de requests (JWT, logging)
- `Program.cs`: Configuración e inyección de dependencias

**Características**:
- Depende de Application e Infrastructure
- Configura servicios y middleware
- Mapea requests HTTP a casos de uso

## Flujo de Dependencias

```
WebAPI → Application → Domain
   ↓
Infrastructure → Application → Domain
```

**Regla de dependencia**: Las capas internas NO conocen las capas externas.

## Flujo de Datos

### Request de API:

1. **Controller** recibe el request HTTP
2. **Middleware** extrae el `clientId` del token JWT
3. **Controller** llama a un **Service** de Application
4. **Service** usa **Repositories** para acceder a datos
5. **Repository** consulta la base de datos
6. Datos fluyen de vuelta transformándose en **DTOs**
7. **Controller** retorna respuesta HTTP

### Ejecución de Agente de IA:

1. **AIAgentService** obtiene datos del **ExternalDataService**
2. **ExternalDataService** consulta el Repositorio 1 (HTTP)
3. **AIAgentService** envía datos a **GeminiAIService**
4. **GeminiAIService** llama a Vertex AI / Gemini
5. **AIAgentService** procesa respuesta de IA
6. **AIAgentService** guarda sugerencias usando **Repository**

## Patrones de Diseño Utilizados

### 1. Repository Pattern
- Abstrae el acceso a datos
- Permite cambiar la implementación de persistencia

### 2. Dependency Injection
- Todas las dependencias se inyectan en constructores
- Configurado en `Program.cs`

### 3. DTO Pattern
- Objetos dedicados para transferencia de datos
- Separan la representación interna de la externa

### 4. Middleware Pattern
- Procesamiento de requests en cadena
- `JwtMiddleware` extrae clientId

### 5. Service Layer Pattern
- Lógica de aplicación encapsulada en servicios
- Orquestación de operaciones complejas

## Seguridad

### Autenticación Multi-Cliente

```
Request → JwtMiddleware → Extract ClientId → HttpContext.Items
                                                      ↓
Controller → GetClientId() → Filter by ClientId → Repository
```

**Garantía**: Cada cliente solo accede a sus propios datos.

### Validación en Capas:

1. **WebAPI**: Validación de entrada, autenticación
2. **Application**: Validación de reglas de negocio
3. **Domain**: Validación de entidades

## Extensibilidad

### Agregar un nuevo tipo de sugerencia:

1. **Domain**: Crear nueva entidad e interface de repositorio
2. **Application**: Crear DTOs e interface de servicio
3. **Infrastructure**: Implementar repositorio y servicio
4. **WebAPI**: Crear controller con endpoints

### Cambiar proveedor de IA:

1. Crear nueva implementación de `IGeminiAIService`
2. Registrar en `Program.cs`
3. No se requieren cambios en otras capas

### Agregar autenticación OAuth:

1. Configurar en `Program.cs`
2. Actualizar `JwtMiddleware` si es necesario
3. No se requieren cambios en lógica de negocio

## Escalabilidad

### Horizontal:
- La API es stateless
- Múltiples instancias pueden ejecutarse detrás de un load balancer
- Base de datos compartida o replicada

### Vertical:
- Async/await en todas las operaciones I/O
- Entity Framework con tracking deshabilitado para lecturas
- Paginación en consultas grandes

### Separación de Responsabilidades:
- Los agentes de IA pueden ejecutarse como jobs independientes
- La API de consulta puede escalar independientemente
- Cache puede agregarse en la capa de Application

## Testing Strategy

### Unit Tests:
- **Domain**: Testar entidades y lógica de dominio
- **Application Services**: Testar con mocks de repositorios

### Integration Tests:
- **Repositories**: Testar con base de datos en memoria
- **Controllers**: Testar endpoints con TestServer

### E2E Tests:
- Flujo completo con base de datos real
- Mocks de servicios externos (Gemini AI, Repositorio 1)

## Consideraciones de Producción

### Configuración:
- Usar Azure Key Vault o AWS Secrets Manager para secretos
- Variables de entorno para configuración por ambiente

### Logging:
- Structured logging con Serilog
- Correlation IDs para tracking de requests
- Log de errores a Application Insights / CloudWatch

### Monitoreo:
- Health checks endpoints
- Métricas de performance (Prometheus)
- Alertas automáticas

### Resiliencia:
- Retry policies con Polly
- Circuit breakers para servicios externos
- Timeouts configurables

## Diagramas

### Diagrama de Capas:

```
┌────────────────────────────────────┐
│         WebAPI Layer               │
│  Controllers, Middleware, Config   │
└────────────────────────────────────┘
              ↓
┌────────────────────────────────────┐
│      Application Layer             │
│   Services, DTOs, Interfaces       │
└────────────────────────────────────┘
              ↓
┌────────────────────────────────────┐
│        Domain Layer                │
│   Entities, Business Rules         │
└────────────────────────────────────┘
              ↑
┌────────────────────────────────────┐
│    Infrastructure Layer            │
│  Repositories, External Services   │
└────────────────────────────────────┘
```

### Flujo de Request:

```
Client → JWT Token → API Gateway
                           ↓
                    [JwtMiddleware]
                           ↓
                    Extract ClientId
                           ↓
                    [Controller]
                           ↓
                    [Service Layer]
                           ↓
                    [Repository]
                           ↓
                    [Database]
```

## Conclusión

Esta arquitectura proporciona:
- ✅ Separación clara de responsabilidades
- ✅ Facilidad de testing
- ✅ Flexibilidad para cambios
- ✅ Escalabilidad horizontal y vertical
- ✅ Mantenibilidad a largo plazo

