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
      Morning.Value.Domain.Test/
      Morning.Value.Application.Test/
      Morning.Value.Infrastructure.Test/
  frontend/
    src/
      Morning.Value.Web.Site/        <-- UI MVC (.NET 8)
    test/
      Morning.Value.Web.Site.Test/
```

---

## Tecnologías
- Frontend: ASP.NET Core MVC (Bootstrap 5)
- Backend: .NET 8, Clean Architecture, EF Core (SQL Server)
- Pruebas: xUnit + FluentAssertions + Moq
- CI: GitHub Actions (restore, build, test, cobertura y cache de NuGet)

---

## Requisitos
- **.NET SDK 8.0**
- **SQL Server (local o remoto)**

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
  Email: root@domain.com
  Clave: admin123
  Acceso a Book Management (alta/consulta).
- **Reader**: ve **Book History** y puede **Borrow/Return**.
  Registro desde SignUp o asignación de rol por DB.
  Acceso a Home de lector, préstamos/devoluciones y historial.
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

## CI (GitHub Actions)

Se agregó un workflow en .github/workflows/ci.yml que valida en cada push/PR a main:

Restore (dotnet restore)

Build en Release (dotnet build --no-restore)

Test por cada proyecto *.Test*.csproj con cobertura XPlat Code Coverage

Coverage report consolidado con ReportGenerator

Artefacto: coverage-html (HTML + Cobertura + resumen Markdown)

El resumen de cobertura se publica en el Job Summary de Actions

NuGet cache con actions/cache

Clave basada en los *.csproj

El paso Cache status muestra claramente el cache hit en cada ejecución

El pipeline también sube el artefacto test-results con los .trx y coverage.cobertura.xml crudos.

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

## Anexos

### Pantallas Compartidas
Sign In
<img width="1416" height="801" alt="image" src="https://github.com/user-attachments/assets/9596c3c2-973c-400c-adee-474f1f9bf143" />

Sign Up
<img width="1418" height="798" alt="image" src="https://github.com/user-attachments/assets/1d09527b-6c00-4739-aa54-fbe6fbadfa5e" />


### Pantallas de rol Reader
Catalogo de Libros Registrados
<img width="1428" height="798" alt="image" src="https://github.com/user-attachments/assets/367f5e12-9b3f-4feb-a78e-9bdd8761bea1" />

Historico de Libros Prestados (El usuario en esta pantalla puede devolver los libros)
<img width="1425" height="799" alt="image" src="https://github.com/user-attachments/assets/723973d9-eddc-478a-af43-724d1425ef6b" />

### Pantallas de rol Admin
Consulta de Libros por Titulo, Autor y Genero. Tambien puede visualizar su Stock
<img width="1417" height="804" alt="image" src="https://github.com/user-attachments/assets/a43a45ae-113d-40bb-b079-81c2234b8732" />

Registrar un nuevo libro
<img width="548" height="562" alt="image" src="https://github.com/user-attachments/assets/65cfc1f0-247b-4aab-9b3f-027699ca40bd" />

### Pantallas del Workflow CI
Resumen General
<img width="1251" height="559" alt="image" src="https://github.com/user-attachments/assets/38f36f01-94a9-4d01-bb80-a7121036d6dd" />

Resumen de Cobertura del Codigo
<img width="541" height="699" alt="image" src="https://github.com/user-attachments/assets/6902ef30-142c-469a-9da8-5e6fd8a9c484" />

Resumen Por capa
<img width="1430" height="749" alt="image" src="https://github.com/user-attachments/assets/a1b295ab-ac53-4248-a81d-8aa7232284d7" />
<img width="1424" height="489" alt="image" src="https://github.com/user-attachments/assets/214ad30d-d272-4f4c-ae85-c4b36154f22d" />
<img width="1418" height="913" alt="image" src="https://github.com/user-attachments/assets/128be526-b49f-4e6b-9a7d-0ccd92a368ed" />

Generacion de 2 Artefactos (No se sube el artefacto compilado puesto que no tenemos CD)
<img width="907" height="259" alt="image" src="https://github.com/user-attachments/assets/f1cfab38-4445-44d1-98e2-fbc5fd956131" />


---

## Licencia

Uso educativo / demo.
