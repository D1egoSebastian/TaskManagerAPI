# TaskManagerAPI

API REST para gestión de tareas con autenticación JWT en ASP.NET Core + Entity Framework Core + SQL Server, junto con un frontend en Next.js.

## ¿Qué hace esta API?

La API permite:

- Registrar usuarios.
- Iniciar sesión y obtener un token JWT + refresh token.
- Crear, listar, consultar, actualizar y eliminar tareas.
- Proteger los endpoints de tareas con JWT.
- Actualizar el estado de las tareas.
- Asegurar que cada usuario solo pueda ver y modificar sus propias tareas.

## Stack técnico

- **Backend**: .NET 10 (ASP.NET Core Web API)
- **Base de datos**: Entity Framework Core + SQL Server
- **Autenticación**: JWT (`JwtBearer`) + Refresh Tokens
- **Hash de contraseñas**: `BCrypt.Net-Next`
- **Frontend**: Next.js 15 + TypeScript + Tailwind CSS
- **Middleware**: Manejo de excepciones personalizado
- **Rate Limiting**: Limitación de solicitudes integrada

## Estructura principal

- `Controllers/AuthController.cs`: registro, login, logout y refresh tokens.
- `Controllers/TasksController.cs`: CRUD de tareas protegido con `[Authorize]`.
- `Data/AppDbContext.cs`: contexto de EF Core.
- `Models/*`: entidades (`User`, `TaskItem`, `RefreshToken`) y enum `TaskStatus`.
- `DTOs/*`: objetos de transferencia de datos.
- `Middleware/ExceptionHandlingMiddleware.cs`: manejo centralizado de errores.
- `taskmanager-ui/`: frontend Next.js con autenticación y dashboard de tareas.

---

## Configuración requerida

En `appsettings.json` debes tener:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TaskManagerAPI;Trusted_Connection=True;TrustServerCertificate=True"
},
"Jwt": {
  "Key": "CLAVE_SUPER_LARGA_MINIMO_32_CARACTERES",
  "Issuer": "TaskManagerAPI",
  "Audience": "TaskManagerAPIUsers",
  "ExpiresMinutes": 60
}
```

> ⚠️ Importante: `Jwt:Key` debe tener al menos 32 caracteres para HS256.

---

## Cómo ejecutar

1. Restaurar paquetes:

```bash
dotnet restore
```

2. Aplicar migraciones (si no está creada la BD):

```bash
dotnet ef database update
```

3. Ejecutar la API:

```bash
dotnet run
```

4. Abrir en navegador:

- Frontend de prueba: `http://localhost:5047/`
- API base: `http://localhost:5047/api/...`

Puertos definidos en `Properties/launchSettings.json`.

---

## Flujo de uso recomendado

1. **Register**
   - `POST /api/auth/register`
   - Body:

```json
{
  "name": "Diego",
  "email": "diego@test.com",
  "password": "123456"
}
```

2. **Login**
   - `POST /api/auth/login`
   - Body:

```json
{
  "email": "diego@test.com",
  "password": "123456"
}
```

   - Respuesta esperada: token JWT + datos básicos del usuario.

3. **Usar token en tareas**
   - Header:

```http
Authorization: Bearer <TOKEN>
```

4. **Endpoints de tareas**
   - `GET /api/tasks`
   - `GET /api/tasks/{id}`
   - `POST /api/tasks`
   - `PUT /api/tasks/{id}`
   - `DELETE /api/tasks/{id}`

---

## Probar desde el frontend incluido

La página en `wwwroot/index.html` te permite:

- Registrar usuario.
- Loguear y guardar token en `localStorage`.
- Crear tareas.
- Listar tareas propias.
- Buscar tarea por ID.
- Eliminar tarea por ID.
- Ver en pantalla el `status` y el JSON de respuesta de cada petición.

---


