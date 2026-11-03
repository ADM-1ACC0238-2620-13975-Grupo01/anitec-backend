# AniTec Platform Backend

Backend ASP.NET Core para AniTec.

<!-- Documentation maintenance marker: pass 12. -->

## Tecnologias

- ASP.NET Core
- Entity Framework Core
- MySQL
- JWT para autenticacion
- BCrypt para hashing de contrasenas
- Repository, Unit of Work, Command Services, Query Services y REST Resources

## Bounded contexts

- `Shared`: base comun, repositorios, Unit of Work, EF Core, middleware y problem details.
- `Iam`: usuarios, sign-in, sign-up, JWT y hashing, basado en el repo del profesor.
- `Profiles`: perfiles de usuario, basado en el repo del profesor.
- `Livestock`: rebanos y animales.
- `Sanitary`: eventos sanitarios e historial medico de animales.
- `Financial`: ingresos y egresos.
- `Activities`: actividades de la granja.
- `Analytics`: metricas de reportes para dashboard.
- `Devices`: balanzas, collares inteligentes, camaras termicas, aretes de identificacion, estaciones meteorologicas y sensores ambientales.
- `Metrics`: lecturas generadas por los dispositivos.
- `Subscriptions`: planes y suscripciones con campos preparados para Stripe.

## Base de datos

El backend usa MySQL. Antes de levantar la API, MySQL debe estar instalado y el servicio debe estar iniciado.

La cadena por defecto esta en `Anitec.Platform/appsettings.Development.json`:

```json
"DefaultConnection": "server=127.0.0.1;port=3306;user=root;password=password;database=anitec-platform"
```

Si tu usuario o contrasena de MySQL son diferentes, cambia ese valor antes de ejecutar el backend.

Para iniciar MySQL en Windows, puedes usar la aplicacion **Services** y verificar que `MySQL80` este en estado `Running`. Tambien puedes probar desde PowerShell como administrador:

```powershell
net start MySQL80
```

La base de datos esperada se llama `anitec-platform`. Si no existe, puedes crearla desde MySQL Workbench o desde consola:

```sql
CREATE DATABASE `anitec-platform`;
```

Al ejecutar la API, Entity Framework Core aplica automaticamente las migraciones pendientes con `context.Database.Migrate()` y luego inserta datos iniciales si la base esta vacia.

Tambien se puede usar la variable de entorno `ANITEC_CONNECTION_STRING` para generar migraciones en otra base.

## Migraciones

Cuando el SDK de .NET este disponible:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project Anitec.Platform --startup-project Anitec.Platform --output-dir Shared/Infrastructure/Persistence/EntityFrameworkCore/Migrations
dotnet ef database update --project Anitec.Platform --startup-project Anitec.Platform
```

Al ejecutar la API, tambien se aplican las migraciones pendientes con `context.Database.Migrate()`, siguiendo el mismo patron del backend usado en clase.

## Ejecucion Del Backend

Desde la carpeta raiz del backend:

```powershell
cd C:\Users\melga\Desktop\TrabajoFinalAppWeb\anitec-backend
dotnet run --project Anitec.Platform
```

La API quedara disponible en:

```text
http://localhost:5191/api/v1
```

Swagger quedara disponible en:

```text
http://localhost:5191/swagger
```

Si Swagger abre con HTTPS, tambien puede aparecer como:

```text
https://localhost:7003/swagger
```

## Pagos De Prueba Con Stripe

Los pagos de suscripcion usan Stripe en modo de prueba. La clave secreta de prueba (`sk_test_...`) no se guarda en el repositorio: `appsettings.json` y `appsettings.Development.json` solo tienen el marcador `change_me`, y el backend responde `400` con "Stripe test secret key is not configured" mientras no haya una clave real.

Para probar los pagos en local, define la clave como variable de entorno en la misma terminal donde se inicia el backend. Escribe tu propia clave de prueba en lugar de `sk_test_...`:

```powershell
$env:StripeSettings__SecretKey='sk_test_...'
dotnet run --project Anitec.Platform
```

- La variable vale solo para esa ventana de terminal; al cerrarla se pierde y no queda en ningun archivo.
- Hay que reiniciar el backend despues de definirla, porque la configuracion se lee al arrancar.
- Usa siempre claves de prueba (`sk_test_...`), nunca las `sk_live_...`. La clave publicable (`pk_...`) y `StripeSettings__WebhookSecret` no se usan.
- Para comprobarlo, ejecuta `POST /api/v1/subscriptions/stripe-checkout` desde Swagger con `{"planId":1,"platform":"android"}`: debe responder `200` con `sessionId` y `checkoutUrl`.
- En el emulador de Android, la aplicacion de depuracion llama a este backend en `http://10.0.2.2:5191` y Stripe vuelve a una pagina del propio backend que abre la aplicacion. Para pagar se usa la tarjeta de prueba `4242 4242 4242 4242`, con cualquier fecha futura y cualquier CVC.
- En el servicio publicado (Render) la clave se configura como la variable de entorno `StripeSettings__SecretKey` del panel Environment.

## Ejecucion Completa Con Frontend

1. Iniciar MySQL.
2. Verificar la cadena de conexion en `Anitec.Platform/appsettings.Development.json`.
3. Levantar el backend:

```powershell
cd C:\Users\melga\Desktop\TrabajoFinalAppWeb\anitec-backend
dotnet run --project Anitec.Platform
```

4. En otra terminal, levantar el frontend:

```powershell
cd C:\Users\melga\Desktop\TrabajoFinalAppWeb\anitec-frontend
npm install
npm run dev
```

5. Abrir el frontend en el navegador:

```text
http://localhost:5173
```

## Usuarios Iniciales

El seed crea usuarios iniciales solo si la base de datos esta vacia. Todos usan la contrasena `anitec123`.

| Rol | Usuario |
| --- | --- |
| Rancher | `ganadero` |
| Rancher | `maria` |
| Rancher | `jose` |
| Rancher | `rosa` |
| Veterinarian | `veterinaria` |
| Veterinarian | `vetpedro` |
| Veterinarian | `vetlucia` |

Tambien se pueden crear usuarios reales desde el frontend en `/iam/sign-up`, seleccionando rol `Rancher` o `Veterinarian`.

## Validacion

Para comprobar que el backend compila:

```powershell
cd C:\Users\melga\Desktop\TrabajoFinalAppWeb\anitec-backend
dotnet build
```

## Pruebas

El proyecto `Anitec.Platform.Tests` (xUnit) contiene tres tipos de pruebas. No necesitan MySQL ni Docker: las de integración y BDD arrancan la API en memoria con una base SQLite (`ASPNETCORE_ENVIRONMENT=Testing`).

| Tipo | Carpeta | Qué verifica |
|---|---|---|
| Unitarias | `Unit/` | Servicios de aplicación (IAM y Livestock), generación y validación de JWT y hash de contraseñas, con repositorios simulados (NSubstitute). |
| Integración | `Integration/` | Endpoints REST reales: autenticación, fincas, corrales, animales (incluidas las operaciones masivas), registros sanitarios y control de acceso por rol. |
| BDD | `Bdd/Features/` | Escenarios Gherkin (`.feature`) ejecutados con Reqnroll sobre la API en memoria. |

```powershell
dotnet test
```
