# AniTec Platform Backend

Backend ASP.NET Core para AniTec.

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
