# README.md — Morning.Value LF2125

# Morning.Value LF2125

Sistema de gestión de biblioteca (ASP.NET Core MVC + **Clean Architecture**) con autenticación por **cookies**, roles **Admin / Reader**, préstamos y devoluciones, historial con filtros/paginación y administración de libros.

## Tabla de contenidos

* [Características](#características)
* [Arquitectura y Estructura](#arquitectura-y-estructura)
* [Teck stack](#tecn-stack)
* [Requisitos](#requisitos)
* [Configuración](#configuración)

  * [Cadenas de conexión](#cadenas-de-conexión)
  * [Variables de entorno (opcional)](#variables-de-entorno-opcional)
* [Ejecución](#ejecución)

  * [Local (sin Docker)](#local-sin-docker)
  * [Docker / Docker Compose](#docker--docker-compose)
* [Migraciones y Seeding](#migraciones-y-seeding)
* [Rutas principales](#rutas-principales)
* [Roles y credenciales de ejemplo](#roles-y-credenciales-de-ejemplo)
* [Pruebas y Cobertura](#pruebas-y-cobertura)
* [CI (GitHub Actions)](#ci-github-actions)
* [Capturas (Anexos)](#capturas-anexos)
* [FAQ / Problemas comunes](#faq--problemas-comunes)
* [Licencia](#licencia)



## Características

* **Usuarios y Roles**: `Admin` (gestiona libros) y `Reader` (solicita/devolver préstamos, consulta historial).
* **Autenticación por Cookies**:

  * `LoginPath = /auth/signin`
  * `LogoutPath = /auth/signout`
* **Autorización**:

  * Política *fallback* (todo requiere autenticación).
  * `[Authorize(Roles="...")]` por controlador/acción.
* **Libros & Préstamos**:

  * Alta/consulta de libros (modal).
  * Préstamos y devoluciones con historial filtrable y paginado.



## Arquitectura y Estructura

**Clean Architecture** separando Dominios / Aplicación / Infraestructura / Web:

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
      Morning.Value.Web.Site/
    test/
      Morning.Value.Web.Site.Test/
```

* **Domain**: Entidades, reglas y excepciones de dominio.
* **Application**: Servicios de aplicación (AppServices), DTOs, puertos (interfaces).
* **Infrastructure**: EF Core (DbContext, UoW, repos), hashing, interceptor de auditoría, **seeding**.
* **Web.Site**: Controladores MVC, Vistas (Razor + Bootstrap), ViewModels.



## Tech stack

* **Frontend:** ASP.NET Core **MVC** (Bootstrap 5)
* **Backend:** .NET **8**, Clean Architecture, **EF Core** (SQL Server)
* **Pruebas:** xUnit + FluentAssertions + Moq
* **CI:** GitHub Actions (restore, build, test, cobertura y cache de NuGet)
* **Contenedores:** Docker & Docker Compose



## Requisitos

* **.NET SDK 8.0**
* **SQL Server** (local, container o remoto)
* **Docker & Docker Compose** (opcional, para levantar stack completo)



## Configuración

### Cadenas de conexión

> Usa **una sola modalidad** de autenticación en la cadena de conexión.
> Si usas `User Id/Password` **no** agregues `Trusted_Connection=True`.

**Ejemplo (SQL Auth, con Docker mapeado a 1443 → 1433):**

```json
{
  "ConnectionStrings": {
    "Local": "Server=localhost,1443;Database=MorningValue;User Id=sa;Password=Admin_sa1!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

**Ejemplo (LocalDB en Windows, sin Docker):**

```json
{
  "ConnectionStrings": {
    "Local": "Server=(localdb)\\MSSQLLocalDB;Database=MorningValue;Trusted_Connection=True;MultipleActiveResultSets=True"
  }
}
```

### Variables de entorno (opcional)

* `ASPNETCORE_ENVIRONMENT` = `Development` | `Production`
* `ConnectionStrings__Default` = *cadena de conexión completa* (sobrescribe `appsettings.json`)



## Ejecución

### Local (sin Docker)

1. Configura `ConnectionStrings:Default` en `apps/frontend/src/Morning.Value.Web.Site/appsettings.Development.json`.
2. Aplica migraciones y (opcional) seed:

   ```bash
   dotnet tool restore
   dotnet ef database update --project apps/backend/src/Morning.Value.Infrastructure --startup-project apps/frontend/src/Morning.Value.Web.Site
   ```
3. Ejecuta la app:

   ```bash
   dotnet run --project apps/frontend/src/Morning.Value.Web.Site
   ```
4. Abre `http://localhost:5080` o el puerto que indique la salida.

### Docker / Docker Compose

> La solución **aplica migraciones y ejecuta el seed inicial** en el arranque.

1. Levanta el stack:

   ```bash
   docker compose up -d --build
   ```
2. Endpoints por defecto:

   * **DB**: `localhost:1443` (conéctate como `localhost,1443`)
   * **Web**: `http://localhost:8080`



## Migraciones y Seeding

* **Migraciones EF Core** (crear/actualizar DB):

  ```bash
  # instalar herramientas si no las tienes
  dotnet tool install --global dotnet-ef

  # agregar una migración
  dotnet ef migrations add v001 \
    --project apps/backend/src/Morning.Value.Infrastructure \
    --startup-project apps/frontend/src/Morning.Value.Web.Site

  # aplicar migraciones
  dotnet ef database update \
    --project apps/backend/src/Morning.Value.Infrastructure \
    --startup-project apps/frontend/src/Morning.Value.Web.Site
  ```

* **Seeding**: en el primer arranque se crean usuarios **Admin** y **Reader** y datos mínimos de prueba.



## Rutas principales

* **Auth**

  * `GET /auth/signin` – formulario de login
  * `POST /auth/signin` – iniciar sesión
  * `GET /auth/signup` – registro
  * `POST /auth/signup` – crear usuario
  * `POST /auth/signout` – cerrar sesión
* **Books (Admin)**

  * `GET /books/management` – búsqueda y alta de libros (modal)
  * `POST /books/create` – crear libro
* **Books (Reader)**

  * `GET /books/history` – historial con filtros/paginación
* **Loans (Reader)**

  * `POST /loans/borrow` – pedir prestado
  * `POST /loans/return` – devolver



## Roles y credenciales de ejemplo

> **Solo para pruebas locales / demo** (creados por el *seeding*).

| Rol    | Email               | Clave       | Acceso                                         |
| ------ | ------------------- | ----------- | ---------------------------------------------- |
| Admin  | `admin@domain.com`  | `admin123`  | Gestión de libros (alta/consulta)              |
| Reader | `reader@domain.com` | `reader123` | Home lector, préstamos/devoluciones, historial |



## Pruebas y Cobertura

Ejecutar todas las pruebas con cobertura:

```bash
dotnet test -c Release --collect:"XPlat Code Coverage"
```

* Archivo Cobertura: `TestResults/**/coverage.cobertura.xml`
* Reporte HTML local (opcional):

  ```bash
  dotnet tool install -g dotnet-reportgenerator-globaltool
  reportgenerator \
    -reports:"**/coverage.cobertura.xml" \
    -targetdir:"coverage-html" \
    -reporttypes:Html
  # abre coverage-html/index.html
  ```



## CI (GitHub Actions)

Workflow en `.github/workflows/ci.yml` (por push/PR a `main`):

* `dotnet restore`
* `dotnet build --configuration Release --no-restore`
* `dotnet test` por cada `*.Test.csproj` con **XPlat Code Coverage**
* Consolidación con **ReportGenerator**
* Publicación de **Job Summary** con resumen de cobertura
* **Cache de NuGet** (muestra cache hit en logs)
* Artefactos:

  * `coverage-html` (HTML + Cobertura + resumen Markdown)
  * `test-results` (`.trx` y `coverage.cobertura.xml`)

## Capturas (Anexos)

### Pantallas Compartidas

**Sign In** <img width="1416" height="801" alt="image" src="https://github.com/user-attachments/assets/9596c3c2-973c-400c-adee-474f1f9bf143" />

**Sign Up** <img width="1418" height="798" alt="image" src="https://github.com/user-attachments/assets/1d09527b-6c00-4739-aa54-fbe6fbadfa5e" />

### Pantallas de rol Reader

**Catálogo de Libros Registrados** <img width="1428" height="798" alt="image" src="https://github.com/user-attachments/assets/367f5e12-9b3f-4feb-a78e-9bdd8761bea1" />

**Histórico de Libros Prestados** *(permite devolver)* <img width="1425" height="799" alt="image" src="https://github.com/user-attachments/assets/723973d9-eddc-478a-af43-724d1425ef6b" />

### Pantallas de rol Admin

**Consulta/Alta de Libros** <img width="1417" height="804" alt="image" src="https://github.com/user-attachments/assets/a43a45ae-113d-40bb-b079-81c2234b8732" />

**Registrar un nuevo libro (modal)** <img width="548" height="562" alt="image" src="https://github.com/user-attachments/assets/65cfc1f0-247b-4aab-9b3f-027699ca40bd" />

### Workflow CI

**Resumen General** <img width="1251" height="559" alt="image" src="https://github.com/user-attachments/assets/38f36f01-94a9-4d01-bb80-a7121036d6dd" />

**Resumen de Cobertura** <img width="541" height="699" alt="image" src="https://github.com/user-attachments/assets/6902ef30-142c-469a-9da8-5e6fd8a9c484" />

**Resumen por Capa** <img width="1430" height="749" alt="image" src="https://github.com/user-attachments/assets/a1b295ab-ac53-4248-a81d-8aa7232284d7" /> <img width="1424" height="489" alt="image" src="https://github.com/user-attachments/assets/214ad30d-d272-4f4c-ae85-c4b36154f22" /> <img width="1418" height="913" alt="image" src="https://github.com/user-attachments/assets/128be526-b49f-4e6b-9a7d-0ccd92a368ed" />

**Artefactos generados** <img width="907" height="259" alt="image" src="https://github.com/user-attachments/assets/f1cfab38-4445-44d1-98e2-fbc5fd956131" />



## FAQ / Problemas comunes

* **No conecta a SQL Server desde host**
  Verifica el *map* de puertos del container (`1443:1433`) y conecta con `Server=localhost,1443`.
* **SSL/Certificado en dev**
  Usa `Encrypt=True;TrustServerCertificate=True` para evitar errores de certificado en entorno local.
* **Migración fallida**
  Asegúrate de ejecutar `dotnet ef` con `--project` (Infrastructure) y `--startup-project` (Web.Site).


## Licencia

**Uso educativo / demo.** Puedes adaptar este código con fines de aprendizaje y pruebas.
