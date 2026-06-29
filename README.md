# 🔐 AuthApi - Backend de Autenticación

Backend de autenticación desarrollado con **.NET 10**, **Entity Framework Core** y **PostgreSQL**. Proporciona autenticación mediante **JWT (JSON Web Tokens)** e integración con **Google OAuth 2.0**.

---

## 📋 Tabla de Contenidos

- [Tecnologías](#-tecnologías)
- [Arquitectura](#-arquitectura)
- [Requisitos Previos](#-requisitos-previos)
- [Configuración](#-configuración)
- [Instalación](#-instalación)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Endpoints de la API](#-endpoints-de-la-api)
- [Flujos de Autenticación](#-flujos-de-autenticación)
- [Seguridad](#-seguridad)
- [Migraciones de Base de Datos](#-migraciones-de-base-de-datos)
- [Despliegue](#-despliegue)
- [Licencia](#-licencia)

---

## 🛠️ Tecnologías

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 10.0 | Framework principal |
| ASP.NET Core | 10.0 | API Web con Minimal APIs |
| Entity Framework Core | 10.0 | ORM para acceso a datos |
| Npgsql | 10.0 | Driver PostgreSQL para .NET |
| ASP.NET Core Identity | 10.0 | Gestión de usuarios, roles y seguridad |
| JWT Bearer | 10.0 | Autenticación stateless con tokens |
| Google OAuth | 10.0 | Autenticación federada con Google |
| PostgreSQL | 16+ | Base de datos relacional |

---

## 🏗️ Arquitectura

```
┌─────────────────┐      HTTP + JWT       ┌─────────────────────┐      SQL      ┌──────────────┐
│   Angular SPA   │ ◄────────────────────► │  .NET 10 Web API    │ ◄──────────► │  PostgreSQL  │
│  (Frontend)     │   Authorization:      │  AuthApi            │   EF Core     │  (Datos)     │
│                 │   Bearer <token>        │                     │               │              │
└─────────────────┘                       └─────────────────────┘               └──────────────┘
         │                                          │
         │                                          │
         └────────── Google OAuth 2.0 ──────────────┘
                        (OpenID Connect)
```

### Patrones Utilizados

- **Minimal APIs**: Endpoints definidos directamente en `Program.cs` mediante extension methods, reduciendo overhead de controladores tradicionales.
- **Repository Pattern implícito**: Entity Framework Core actúa como capa de abstracción de datos.
- **Dependency Injection**: Servicios registrados con ciclos de vida apropiados (`Scoped`, `Singleton`).
- **DTO Pattern**: Separación entre modelos de dominio y contratos de API.
- **JWT Stateles**: Sin sesiones en servidor; cada petición es autónoma y verificable.

---

## 📦 Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Google Cloud Console](https://console.cloud.google.com/) (para OAuth 2.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/) (opcional)

### Verificar instalación

```bash
dotnet --version        # Debe mostrar 10.0.x
psql --version          # PostgreSQL 16+
```

---

## ⚙️ Configuración

### 1. Configurar PostgreSQL

Crear base de datos y usuario:

```sql
CREATE DATABASE authapp;
CREATE USER appuser WITH ENCRYPTED PASSWORD 'tu_password_segura';
GRANT ALL PRIVILEGES ON DATABASE authapp TO appuser;
```

### 2. Configurar Google OAuth 2.0

1. Accede a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto o selecciona uno existente
3. Ve a **APIs & Services > Credentials**
4. Crea una credencial tipo **OAuth 2.0 Client ID**
   - Tipo de aplicación: **Web application**
   - Orígenes autorizados de JavaScript: `http://localhost:4200`
   - URIs de redireccionamiento autorizados: `http://localhost:5000/api/auth/google-callback`
5. Copia el **Client ID** y **Client Secret**

### 3. Configurar `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=authapp;Username=appuser;Password=tu_password_segura"
  },
  "JwtSettings": {
    "Key": "tu-clave-super-secreta-de-al-menos-32-caracteres!!",
    "Issuer": "AuthApi",
    "Audience": "AuthApp",
    "ExpiryMinutes": 60
  },
  "Google": {
    "ClientId": "tu-client-id.apps.googleusercontent.com",
    "ClientSecret": "tu-client-secret"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ⚠️ **IMPORTANTE**: En producción, nunca almacenes secretos en `appsettings.json`. Utiliza variables de entorno, Azure Key Vault, AWS Secrets Manager o similar.

### Variables de Entorno (Producción)

```bash
export ConnectionStrings__DefaultConnection="Host=..."
export JwtSettings__Key="tu-clave-super-secreta-de-al-menos-32-caracteres!!"
export Google__ClientId="..."
export Google__ClientSecret="..."
```

---

## 🚀 Instalación

### Clonar y restaurar dependencias

```bash
git clone <url-del-repositorio>
cd AuthApi
dotnet restore
```

### Aplicar migraciones de base de datos

```bash
dotnet tool install --global dotnet-ef  # Si no está instalado
dotnet ef migrations add InitialCreate   # Crear migración (solo primera vez)
dotnet ef database update                # Aplicar a PostgreSQL
```

### Ejecutar en desarrollo

```bash
dotnet run
# o
dotnet watch run  # Con hot reload
```

La API estará disponible en:
- `http://localhost:5000`
- `https://localhost:5001` (si hay certificado HTTPS)

### Documentación Swagger

Accede a la documentación interactiva en:
```
http://localhost:5000/swagger
```

---

## 📁 Estructura del Proyecto

```
AuthApi/
│
├── 📄 Program.cs                    # Punto de entrada, configuración de servicios y middleware
├── 📄 appsettings.json              # Configuración de la aplicación
├── 📄 appsettings.Development.json  # Configuración específica de desarrollo
├── 📄 AuthApi.csproj                # Archivo de proyecto .NET
│
├── 📁 Data/
│   ├── 📄 AppDbContext.cs           # DbContext de Entity Framework Core
│   └── 📄 SeedData.cs               # Datos iniciales (roles, usuario admin)
│
├── 📁 Models/
│   └── 📄 ApplicationUser.cs        # Modelo de usuario extendido de IdentityUser
│
├── 📁 Endpoints/
│   └── 📄 AuthEndpoints.cs          # Definición de endpoints de autenticación (Minimal APIs)
│
├── 📁 Services/
│   ├── 📄 TokenService.cs           # Generación de tokens JWT
│   └── 📄 ITokenService.cs          # Interfaz del servicio de tokens
│
├── 📁 Migrations/                   # Migraciones de Entity Framework (generadas automáticamente)
│
└── 📁 Properties/
    └── 📄 launchSettings.json       # Configuración de lanzamiento
```

### Descripción de capas

| Carpeta | Responsabilidad |
|---------|-----------------|
| `Data/` | Contexto de base de datos, configuración de EF Core y seed de datos |
| `Models/` | Entidades de dominio que se mapean a tablas de PostgreSQL |
| `Endpoints/` | Definición de rutas HTTP con Minimal APIs |
| `Services/` | Lógica de negocio reutilizable (generación de tokens, etc.) |

---

## 🔌 Endpoints de la API

### Base URL
```
http://localhost:5000/api/auth
```

### Endpoints Públicos

#### 🔹 Registro de usuario
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "ContraseñaSegura123!",
  "firstName": "Juan",
  "lastName": "Pérez"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Usuario registrado exitosamente"
}
```

**Respuesta error (400 Bad Request):**
```json
{
  "errors": [
    "Passwords must have at least one non alphanumeric character.",
    "Passwords must have at least one uppercase ('A'-'Z')."
  ]
}
```

---

#### 🔹 Inicio de sesión con email/password
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "ContraseñaSegura123!"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "email": "usuario@ejemplo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "roles": ["User"],
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-06-29T11:30:00Z"
}
```

**Respuesta error (401 Unauthorized):**
```json
{
  "message": "Credenciales inválidas"
}
```

> 🔒 **Nota de seguridad**: El backend devuelve el mismo mensaje de error tanto si el email no existe como si la contraseña es incorrecta. Esto previene la enumeración de usuarios.

---

#### 🔹 Iniciar autenticación con Google
```http
GET /api/auth/google-login
```

Redirige al usuario a Google para autenticarse. No requiere body.

---

#### 🔹 Callback de Google OAuth
```http
GET /api/auth/google-callback?token=eyJhbGciOiJIUzI1NiIs...
```

Endpoint interno al que Google redirige después de la autenticación. El frontend debe manejar la redirección final con el token JWT.

---

### Endpoints Protegidos (requieren Bearer Token)

#### 🔹 Obtener usuario actual
```http
GET /api/auth/me
Authorization: Bearer <token_jwt>
```

**Respuesta exitosa (200 OK):**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "email": "usuario@ejemplo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "roles": ["User"]
}
```

**Respuesta error (401 Unauthorized):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401
}
```

---

## 🔐 Flujos de Autenticación

### 1. Registro e Inicio de Sesión con Email/Password

```
┌─────────┐                                    ┌─────────┐                                    ┌─────────┐
│ Cliente │                                    │  API    │                                    │   DB    │
└────┬────┘                                    └────┬────┘                                    └────┬────┘
     │                                              │                                              │
     │  POST /api/auth/register                     │                                              │
     │  { email, password, firstName, lastName }    │                                              │
     │ ─────────────────────────────────────────────>│                                              │
     │                                              │  Verificar email único                       │
     │                                              │  Hashear contraseña (PBKDF2/bcrypt)         │
     │                                              │  Guardar usuario en PostgreSQL               │
     │                                              │ ────────────────────────────────────────────>│
     │                                              │                                              │
     │  200 OK { message }                          │                                              │
     │ <─────────────────────────────────────────────│                                              │
     │                                              │                                              │
     │  POST /api/auth/login                        │                                              │
     │  { email, password }                         │                                              │
     │ ─────────────────────────────────────────────>│                                              │
     │                                              │  Buscar usuario por email                    │
     │                                              │  Verificar hash de contraseña                │
     │                                              │  Generar JWT con claims (id, email, roles)   │
     │                                              │                                              │
     │  200 OK { token, user, expiresAt }           │                                              │
     │ <─────────────────────────────────────────────│                                              │
     │                                              │                                              │
```

### 2. Autenticación con Google OAuth 2.0

```
┌─────────┐      ┌─────────┐      ┌─────────┐                                    ┌─────────┐
│ Cliente │      │  API    │      │ Google  │                                    │   DB    │
└────┬────┘      └────┬────┘      └────┬────┘                                    └────┬────┘
     │                │                │                                              │
     │  GET /api/auth/google-login     │                                              │
     │ ───────────────────────────────>│                                              │
     │                │                │                                              │
     │                │  Redirect a Google OAuth                                     │
     │                │ ───────────────────────────────>│                            │
     │                │                │                                              │
     │                │                │  Usuario inicia sesión en Google             │
     │                │                │  Google verifica credenciales                │
     │                │                │                                              │
     │                │  Redirect a /api/auth/google-callback                         │
     │                │ <───────────────────────────────│                            │
     │                │                │                                              │
     │                │  Obtener datos del usuario (email, nombre, id)             │
     │                │  Buscar/crear usuario en PostgreSQL                          │
     │                │  Generar JWT                                 │
     │                │ ────────────────────────────────────────────────────────────>│
     │                │                │                                              │
     │                │  Redirect a frontend con token                                 │
     │                │  Location: http://localhost:4200/auth/callback?token=xxx     │
     │ <──────────────│                │                                              │
     │                │                │                                              │
```

### 3. Acceso a Recursos Protegidos

```
┌─────────┐                                    ┌─────────┐
│ Cliente │                                    │  API    │
└────┬────┘                                    └────┬────┘
     │                                              │
     │  GET /api/auth/me                            │
     │  Authorization: Bearer <token_jwt>           │
     │ ─────────────────────────────────────────────>│
     │                                              │  Validar firma del JWT
     │                                              │  Verificar expiración
     │                                              │  Extraer claims (sub, email, role)
     │                                              │  Buscar usuario en base de datos
     │                                              │
     │  200 OK { user }                             │
     │ <─────────────────────────────────────────────│
     │                                              │
```

---

## 🛡️ Seguridad

### Políticas de Contraseñas

| Requisito | Valor | Descripción |
|-----------|-------|-------------|
| Longitud mínima | 8 caracteres | Evita contraseñas cortas |
| Dígitos | Requerido | Al menos un número (0-9) |
| Minúsculas | Requerido | Al menos una letra minúscula |
| Mayúsculas | Requerido | Al menos una letra mayúscula |
| Caracteres especiales | Requerido | Al menos un símbolo (!@#$%^&*) |

### Protección contra Fuerza Bruta

| Configuración | Valor | Descripción |
|---------------|-------|-------------|
| Intentos máximos fallidos | 5 | Bloqueo tras 5 intentos consecutivos |
| Tiempo de bloqueo | 15 minutos | Ventana de tiempo antes de reintentar |

### Configuración JWT

| Parámetro | Valor Recomendado | Descripción |
|-----------|-------------------|-------------|
| `Key` | ≥ 32 caracteres (256 bits) | Clave simétrica para firma HMAC-SHA256 |
| `Issuer` | Nombre de tu API | Identifica quién emitió el token |
| `Audience` | Nombre de tu app | Identifica para quién es el token |
| `ExpiryMinutes` | 15-60 minutos | Tiempo de vida del token de acceso |
| `ClockSkew` | 0 segundos | Sin tolerancia en expiración |

### Headers de Seguridad

La API configura automáticamente:
- **CORS**: Solo permite orígenes explícitamente configurados
- **HTTPS Redirection**: Redirige automáticamente HTTP a HTTPS
- **Authentication/Authorization**: Middleware en orden correcto (Authentication antes que Authorization)

### Recomendaciones para Producción

1. **HTTPS obligatorio**: Nunca transmitas tokens por HTTP
2. **Refresh Tokens**: Implementa tokens de refresco para renovar access tokens sin re-login
3. **Rate Limiting**: Limita peticiones por IP/usuario
4. **Secrets Management**: Usa Azure Key Vault, AWS Secrets Manager o variables de entorno
5. **XSS Protection**: Considera cookies `HttpOnly` + `SameSite=Strict` en lugar de `localStorage`
6. **Email Confirmation**: Activa `RequireConfirmedEmail = true`
7. **Auditoría**: Loguea todos los intentos de login (exitosos y fallidos)

---

## 🗄️ Migraciones de Base de Datos

### Tablas Generadas por ASP.NET Core Identity

| Tabla | Descripción |
|-------|-------------|
| `aspnetusers` | Usuarios registrados |
| `aspnetroles` | Roles del sistema (Admin, User, Manager) |
| `aspnetuserroles` | Relación muchos-a-muchos usuario-rol |
| `aspnetuserclaims` | Claims adicionales por usuario |
| `aspnetuserlogins` | Logins externos (Google, Facebook, etc.) |
| `aspnetusertokens` | Tokens de recuperación de contraseña, 2FA |
| `aspnetroleclaims` | Claims asociados a roles |

### Comandos Útiles

```bash
# Crear nueva migración
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a una migración específica
dotnet ef database update NombreMigracionAnterior

# Generar script SQL
dotnet ef migrations script -o script.sql

# Eliminar última migración (si no aplicada)
dotnet ef migrations remove
```

---

## 🚀 Despliegue

### Docker (Recomendado)

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AuthApi.csproj", "."]
RUN dotnet restore "AuthApi.csproj"
COPY . .
RUN dotnet build "AuthApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthApi.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=authapp;Username=postgres;Password=postgres
      - JwtSettings__Key=${JWT_KEY}
      - Google__ClientId=${GOOGLE_CLIENT_ID}
      - Google__ClientSecret=${GOOGLE_CLIENT_SECRET}
    depends_on:
      - db

  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: authapp
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  postgres_data:
```

### Azure App Service

```bash
# Publicar
az webapp up --name mi-auth-api --resource-group mi-grupo --runtime "DOTNETCORE:10.0"

# Configurar variables de entorno
az webapp config appsettings set --name mi-auth-api --settings   ConnectionStrings__DefaultConnection="..."   JwtSettings__Key="..."   Google__ClientId="..."
```

---

## 🧪 Testing

### Ejecutar tests

```bash
dotnet test
```

### Tests recomendados a implementar

- **Unit tests**: Servicios (`TokenService`), validaciones de DTOs
- **Integration tests**: Endpoints con `WebApplicationFactory`, base de datos en memoria
- **Security tests**: Intento de acceso sin token, token expirado, token manipulado

---

## 📚 Recursos Adicionales

- [Documentación oficial de .NET 10](https://learn.microsoft.com/en-us/aspnet/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT en ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-auth)
- [Google OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/efcore/)

---

## 🤝 Contribución

1. Fork el repositorio
2. Crea una rama feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto está licenciado bajo la [MIT License](LICENSE).

---

<div align="center">
  <sub>Desarrollado con ❤️ usando .NET 10, PostgreSQL y Angular</sub>
</div>