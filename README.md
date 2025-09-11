# Morning.Value LF2125

Sistema de biblioteca (demo) construido con **ASP.NET Core 8 MVC** siguiendo una **arquitectura limpia**: Dominio, Aplicación, Infraestructura y Web (UI).  
Permite:

- **Autenticación** con cookies y **roles** (`Admin`, `Reader`).
- **Gestión de libros** (Admin): crear y consultar.
- **Préstamos** (Reader): pedir prestado / devolver.
- **Historial** de préstamos con filtros y paginación.
- **EF Core** (Code-First) con auditoría de creación/modificación.
- **Tests** de Dominio, Aplicación e Infraestructura.

---

## Estructura

```
apps/
  backend/
    src/
      Morning.Value.Domain/
      Morning.Value.Application/
      Morning.Value.Infrastructure/
    test/
      Morning.Value.Domain.Tests/
      Morning.Value.Application.Tests/
      Morning.Value.Infrastructure.Tests/
  frontend/
    src/
      Morning.Value.Web.Site/        <-- UI MVC (.NET 8)
```

---

## Requisitos

- **.NET SDK 8.0**
- (Opcional) **EF Core Tools**: `dotnet tool install -g dotnet-ef`

---

## Configuración de base de datos

### Opción A: SQL Server local (recomendado para desarrollo)

1. Habilita **TCP 1433** en SQL Server y crea un **login SQL**.
2. Configura tu _connection string_ en `apps/frontend/src/Morning.Value.Web.Site/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MorningValue;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

> Si usas **Windows Authentication** o **instancia nombrada**, ajusta el `Server` en consecuencia.

3. Aplica migraciones (Code-First):

```bash
# Desde la raíz del repo
dotnet restore
dotnet build

# Aplica migraciones usando el proyecto Web como startup
dotnet ef database update   --project apps/backend/src/Morning.Value.Infrastructure   --startup-project apps/frontend/src/Morning.Value.Web.Site
```

## Ejecución

```bash
dotnet restore
dotnet build
dotnet ef database update   --project apps/backend/src/Morning.Value.Infrastructure   --startup-project apps/frontend/src/Morning.Value.Web.Site

dotnet run --project apps/frontend/src/Morning.Value.Web.Site
```

- UI: `http://localhost:5080` (o el puerto que indique la consola)
- Ruta de inicio de sesión: `/auth/signin`

> La app usa **cookies** y política de autorización global: si no estás autenticado, te redirige a `/auth/signin`.

---

## Rutas principales

- **Auth**
  - `GET /auth/signin` – formulario de login
  - `POST /auth/signin` – iniciar sesión
  - `GET /auth/signup` – registro
  - `POST /auth/signup` – crear usuario
  - `POST /auth/signout` – cerrar sesión
- **Books (Admin)**
  - `GET /books/management` – búsqueda y alta de libros (modal)
  - `POST /books/create` – crear libro
- **Books (Reader)**
  - `GET /books/history` – historial con filtros/paginación
- **Loans (Reader)**
  - `POST /loans/borrow` – pedir prestado
  - `POST /loans/return` – devolver

---

## Roles y navegación

- **Admin**: ve y accede a **Book Management**.
- **Reader**: ve **Book History** y puede **Borrow/Return**.
- La UI oculta menús según rol y los controladores usan `[Authorize(Roles="...")]`.

---

## Tests

```bash
dotnet test
# o por proyecto:
dotnet test apps/backend/test/Morning.Value.Domain.Tests
dotnet test apps/backend/test/Morning.Value.Application.Tests
dotnet test apps/backend/test/Morning.Value.Infrastructure.Tests
```

---

## Problemas comunes

- **SqlException: error 40** (no se puede conectar):
  - Si corres el sitio en Docker: usa `Server=host.docker.internal,1433` y habilita TCP en SQL Server.
  - Si usas instancia nombrada, fija puerto 1433 o usa el puerto real.
- **HTTPS en contenedor**:
  - En dev sirve por HTTP: `ASPNETCORE_URLS=http://+:8080`.
- **Migraciones**:
  - Asegúrate de ejecutar `dotnet ef database update` con `--startup-project` apuntando al **Web.Site**.

---

## Usuario administrador (seed)

Para contar con un **usuario Admin** inicial, ejecuta el siguiente script SQL sobre la base de datos **MorningValue** (por ejemplo con SQL Server Management Studio o `sqlcmd`).

> Este usuario podrá ingresar al sistema con el correo y clave indicados abajo.  
> **Importante:** el hash de contraseña corresponde a la clave `admin123`. Si cambias el algoritmo de hash, actualiza este valor.

```sql
INSERT INTO [dbo].[Users]
           ([Id]
           ,[Name]
           ,[Email]
           ,[PasswordHash]
           ,[Role]
           ,[CreatedAtUtc]
           ,[CreatedBy]
           ,[ModifiedAtUtc]
           ,[ModifiedBy])
     VALUES
           ('BFE0136B-0790-4969-9D2C-08DDF129FE59'
           ,'Administrador'
           ,'root@domain.com'
           ,'AQAAAAIAAYagAAAAEAoAwF2aNxwwshXq7xK5EG2yvxDAOAIeg5XAePz4hvWNvLLpcQXALFlsR85qSE5nzA=='
           ,1
           ,'2025-09-11'
           ,'SYSTEM_USER'
           ,NULL
           ,NULL);
```

**Credenciales**
- **Email:** `root@domain.com`
- **Contraseña:** `admin123`

---

## Licencia

Uso educativo / demo.
