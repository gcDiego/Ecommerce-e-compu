# Ecommerce Paduki

Guía para configurar y ejecutar localmente los componentes ya disponibles del proyecto.

Para continuar el trabajo de migración desde otra conversión o equipo, leer primero `HANDOFF_MIGRACION.md`.

## Componentes disponibles

- Aplicaciones heredadas ASP.NET MVC sobre .NET Framework 4.7.2.
- Catalog Service sobre .NET 8.
- Identity Service sobre .NET 8.
- Cart Service sobre .NET 8.
- SQL Server con la base de datos existente `ecommerce`.

Catalog Service ofrece las lecturas de categorías, marcas y productos. Identity Service ofrece autenticación JWT, rehash progresivo de contraseñas heredadas y cambio autenticado de contraseña.

## Requisitos

### Para los microservicios

- .NET SDK 8.0.420 o un parche compatible con `global.json`.
- SQL Server 2022, local o mediante Docker.
- Una copia autorizada de la base `ecommerce`.
- Git.
- Docker Desktop, sólo si SQL Server se ejecutará en un contenedor.

### Para las aplicaciones MVC heredadas

Una de estas alternativas:

- Windows con Visual Studio, la carga de trabajo **ASP.NET y desarrollo web** y .NET Framework 4.7.2 Developer Pack.
- macOS con Mono 6.12, MSBuild para Mono 16.10 y XSP4, según `EJECUCION_MONO_MACOS.md`.
- SQL Server accesible desde el sistema elegido.

Los proyectos MVC no se ejecutan con `dotnet run`. En macOS se validaron específicamente mediante Mono/MSBuild y XSP4; esta ruta es temporal para comparación durante la migración.

## Obtener el proyecto

```bash
git clone <URL_DEL_REPOSITORIO>
```

Después, entrar en la carpeta `ecommerce` del repositorio.

## Preparar SQL Server

Los servicios no crean ni modifican automáticamente el esquema. La instancia debe contener previamente la base `ecommerce` con sus tablas y datos requeridos.

Actualmente el repositorio no incluye un respaldo ni scripts completos para reconstruir la base. Cada desarrollador debe usar una copia autorizada y sanitizada o restaurar un respaldo suministrado por el responsable del proyecto.

### SQL Server mediante Docker

Ejemplo para crear un contenedor vacío:

```bash
docker run --name ecommerce-sql \
  -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD='<CONTRASEÑA_LOCAL_SEGURA>' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Este comando inicia SQL Server, pero **no crea la base `ecommerce`**. Después debe restaurarse el respaldo autorizado mediante Azure Data Studio, SQL Server Management Studio o `sqlcmd`.

La contraseña debe cumplir la política de SQL Server y no debe guardarse en Git.

## Configuración segura

No escribir cadenas de conexión ni claves JWT reales en `appsettings.json`. Los servicios leen valores externos mediante variables de entorno.

### Variables requeridas

| Servicio | Variable | Descripción |
|---|---|---|
| Catalog | `ConnectionStrings__CatalogDatabase` | Conexión a la base `ecommerce` |
| Catalog | `Jwt__SigningKey` | Clave usada para validar tokens de Identity |
| Identity | `ConnectionStrings__IdentityDatabase` | Conexión a la base `ecommerce` |
| Identity | `Jwt__SigningKey` | Clave usada para firmar tokens |
| Cart | `ConnectionStrings__CartDatabase` | Conexión a la base `ecommerce` |
| Cart | `Jwt__SigningKey` | Misma clave usada por Identity |

`Jwt__SigningKey` debe tener al menos 32 caracteres y debe ser exactamente la misma en Catalog, Identity y Cart.

El emisor predeterminado es `Ecommerce.Identity` y la audiencia predeterminada es `Ecommerce.Services`. Se pueden sobrescribir con `Jwt__Issuer` y `Jwt__Audience`, manteniendo los mismos valores en los tres servicios.

### Generar una clave JWT local

macOS o Linux:

```bash
openssl rand -hex 32
```

PowerShell:

```powershell
$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToHexString($bytes)
```

No compartir ni versionar el valor generado.

## Iniciar los microservicios en macOS o Linux

Abrir una terminal para Catalog Service:

```bash
export ConnectionStrings__CatalogDatabase='Server=localhost,1433;Database=ecommerce;User Id=sa;Password=<CONTRASEÑA_SQL>;Encrypt=True;TrustServerCertificate=True'
export Jwt__SigningKey='<MISMA_CLAVE_JWT_EN_LOS_TRES_SERVICIOS>'
dotnet run --project src/Services/Catalog/Catalog.Api/Catalog.Api.csproj
```

Abrir otra terminal para Identity Service:

```bash
export ConnectionStrings__IdentityDatabase='Server=localhost,1433;Database=ecommerce;User Id=sa;Password=<CONTRASEÑA_SQL>;Encrypt=True;TrustServerCertificate=True'
export Jwt__SigningKey='<MISMA_CLAVE_JWT_EN_LOS_TRES_SERVICIOS>'
dotnet run --project src/Services/Identity/Identity.Api/Identity.Api.csproj
```

Abrir otra terminal para Cart Service:

```bash
export ConnectionStrings__CartDatabase='Server=localhost,1433;Database=ecommerce;User Id=sa;Password=<CONTRASEÑA_SQL>;Encrypt=True;TrustServerCertificate=True'
export Jwt__SigningKey='<MISMA_CLAVE_JWT_EN_LOS_TRES_SERVICIOS>'
dotnet run --project src/Services/Cart/Cart.Api/Cart.Api.csproj
```

## Iniciar los microservicios en Windows PowerShell

Abrir una terminal para Catalog Service:

```powershell
$env:ConnectionStrings__CatalogDatabase = 'Server=localhost,1433;Database=ecommerce;User Id=sa;Password=<CONTRASEÑA_SQL>;Encrypt=True;TrustServerCertificate=True'
$env:Jwt__SigningKey = '<MISMA_CLAVE_JWT_EN_LOS_TRES_SERVICIOS>'
dotnet run --project src/Services/Catalog/Catalog.Api/Catalog.Api.csproj
```

Abrir otra terminal para Identity Service:

```powershell
$env:ConnectionStrings__IdentityDatabase = 'Server=localhost,1433;Database=ecommerce;User Id=sa;Password=<CONTRASEÑA_SQL>;Encrypt=True;TrustServerCertificate=True'
$env:Jwt__SigningKey = '<MISMA_CLAVE_JWT_EN_LOS_TRES_SERVICIOS>'
dotnet run --project src/Services/Identity/Identity.Api/Identity.Api.csproj
```

Abrir otra terminal para Cart Service:

```powershell
$env:ConnectionStrings__CartDatabase = 'Server=localhost,1433;Database=ecommerce;User Id=sa;Password=<CONTRASEÑA_SQL>;Encrypt=True;TrustServerCertificate=True'
$env:Jwt__SigningKey = '<MISMA_CLAVE_JWT_EN_LOS_TRES_SERVICIOS>'
dotnet run --project src/Services/Cart/Cart.Api/Cart.Api.csproj
```

## Direcciones locales

| Componente | Dirección predeterminada |
|---|---|
| Catalog Service | `http://localhost:5137` |
| Catalog Swagger, en Development | `http://localhost:5137/swagger` |
| Identity Service | `http://localhost:5204` |
| Cart Service | `http://localhost:5292` |
| Catalog health check | `http://localhost:5137/health` |
| Identity health check | `http://localhost:5204/health` |
| Cart health check | `http://localhost:5292/health` |

Los puertos proceden de los perfiles `http` en `Properties/launchSettings.json`.

## Verificar los servicios

```bash
curl -i http://localhost:5137/health
curl -i http://localhost:5204/health
curl -i http://localhost:5292/health
curl -i http://localhost:5137/api/v1/categories
```

Los health checks deben responder `200 OK`. Las lecturas actuales de Catalog son públicas. Las operaciones protegidas deben usar un JWT emitido por Identity Service.

## Ejecutar las pruebas

Desde la carpeta `ecommerce`:

```bash
dotnet test Ecommerce.Services.sln --configuration Release
```

La suite actual incluye pruebas unitarias y HTTP de Catalog, Identity y Cart.

## Configurar la tienda MVC heredada

La tienda puede ejecutarse en Windows con Visual Studio o en macOS mediante el procedimiento validado en `EJECUCION_MONO_MACOS.md`.

1. Abrir la solución heredada correspondiente en Visual Studio.
2. Restaurar los paquetes NuGet.
3. Configurar la cadena `cadena` de `CapaPresentacionTienda/Web.config` para apuntar al SQL Server local.
4. Para usar Catalog Service, configurar:

```xml
<add key="Features:UseCatalogApi" value="true"/>
<add key="CatalogApi:BaseUrl" value="http://localhost:5137/"/>
```

5. Iniciar Catalog Service antes de abrir las pantallas de catálogo.
6. Para volver temporalmente al acceso legado, establecer `Features:UseCatalogApi` en `false`.

No confirmar en Git cadenas de conexión, contraseñas, tokens ni credenciales de proveedores externos.

## Orden recomendado de inicio

1. SQL Server.
2. Restaurar o verificar la base `ecommerce`.
3. Identity Service.
4. Cart Service.
5. Catalog Service.
6. Aplicación MVC heredada, únicamente si se necesita probar la interfaz.

Catalog, Identity y Cart pueden iniciarse en cualquier orden, pero todos deben usar la misma configuración JWT.

## Solución de problemas

### Falta la cadena de conexión

El servicio se detiene al iniciar con un mensaje indicando que `CatalogDatabase`, `IdentityDatabase` o `CartDatabase` no está configurada. Definir la variable `ConnectionStrings__...` en la misma terminal donde se ejecuta `dotnet run`.

### La clave JWT no está configurada

Definir `Jwt__SigningKey` con al menos 32 caracteres. Usar el mismo valor para los tres servicios.

### El health check responde `Unhealthy`

Comprobar:

- Que SQL Server esté iniciado.
- Que el puerto sea correcto.
- Que la base `ecommerce` exista.
- Que el usuario tenga acceso a la base.
- Que `TrustServerCertificate=True` esté presente para el certificado local.

### Un token de Identity recibe `401` en otro servicio

Comprobar que Identity, Catalog y Cart tengan exactamente los mismos valores para:

- `Jwt__SigningKey`.
- `Jwt__Issuer`.
- `Jwt__Audience`.

También verificar que el token no haya expirado y que los relojes de ambas máquinas estén sincronizados.

### Los proyectos MVC no compilan con `dotnet build`

Es el comportamiento esperado para estos proyectos ASP.NET MVC sobre .NET Framework. Usar Visual Studio en Windows o Mono/MSBuild en macOS. Para macOS, seguir `EJECUCION_MONO_MACOS.md`.

## Seguridad

- No guardar secretos en `appsettings.json`, `Web.config`, scripts, documentación o commits.
- No compartir JWT, contraseñas o cadenas completas por chat.
- Usar credenciales exclusivas para desarrollo.
- Rotar cualquier secreto que se haya expuesto.
- No distribuir respaldos que contengan datos personales sin autorización y sanitización.
