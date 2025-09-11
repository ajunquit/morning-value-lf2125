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
- **SQL Server** (local o en contenedor)
- (Opcional) **Docker** y **Docker Compose**
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

### Opción B: SQL Server en Docker

`docker-compose.yml` mínimo:

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "Your_strong_Passw0rd"
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
volumes:
  sql-data:
```

Levanta la BD:

```bash
docker compose up -d
```

Configura la cadena de conexión igual que en la opción A (Server=localhost,1433).

---

## Ejecución (sin Docker)

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

## Ejecución con Docker (solo Web)

> Útil cuando ya tienes SQL Server local corriendo.

Build & run:

```bash
# Build de la imagen del sitio
docker build -t mv-frontend   -f apps/frontend/src/Morning.Value.Web.Site/Dockerfile .

# Ejecutar (apunta a tu SQL local)
docker run --rm -it -p 8080:8080   -e ASPNETCORE_ENVIRONMENT=Development   -e ASPNETCORE_URLS=http://+:8080   -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=MorningValue;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"   mv-frontend
```

- UI: `http://localhost:8080`
- Nota: `host.docker.internal` permite que el contenedor alcance tu SQL Server del host.

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

## Licencia

Uso educativo / demo.
