# Contrato de sincronización y consentimiento veterinario

Resumen para quien consume la API desde Android o Flutter (Sprint 3: US-020 a US-022, TS-007, US-032).
Los detalles de cada endpoint están en Swagger.

## 1. Consentimiento del ganadero sobre el veterinario

Solo un vínculo **Accepted** da acceso a los datos del ganadero.

| Estado | Significado |
|---|---|
| `Pending` | El veterinario pidió acceso y el ganadero aún no responde. Sin acceso. |
| `Accepted` | El ganadero aceptó. El veterinario ve y registra datos de ese ganadero. |
| `Rejected` | El ganadero rechazó la solicitud. Sin acceso. |
| `Revoked` | El ganadero retiró un acceso que estaba activo. Sin acceso. |

Transiciones: `Pending → Accepted | Rejected`, `Accepted → Revoked`. Un veterinario cuya solicitud fue rechazada o revocada puede pedir de nuevo (el mismo vínculo vuelve a `Pending`).

| Quién | Endpoint | Resultado |
|---|---|---|
| Veterinario | `POST /api/v1/veterinarian/{vetId}/clients/{rancherId}` | `201` con el vínculo en `Pending`; `404` si el usuario no es ganadero; `409` si ya hay una solicitud pendiente o un acceso activo |
| Veterinario | `GET /api/v1/veterinarian/{vetId}/clients` | Todos sus vínculos con su estado |
| Veterinario | `GET /api/v1/veterinarian/{vetId}/available-ranchers` | Ganaderos a los que puede pedir acceso (sin solicitud pendiente ni acceso activo) |
| Veterinario | `DELETE /api/v1/veterinarian/{vetId}/clients/{rancherId}` | Retira la solicitud o el vínculo, sea cual sea su estado |
| Ganadero | `GET /api/v1/rancher/{rancherId}/veterinarian-links?status=Pending` | Solicitudes y vínculos con la identidad del veterinario |
| Ganadero | `POST …/veterinarian-links/{linkId}/accept` | `200`; `409` si la solicitud ya fue respondida |
| Ganadero | `POST …/veterinarian-links/{linkId}/reject` | `200`; `409` si la solicitud ya fue respondida |
| Ganadero | `POST …/veterinarian-links/{linkId}/revoke` | `200`; `404` con mensaje si no hay acceso activo |

Al revocar, el veterinario también sale de los rebaños del ganadero que lo nombran, y deja de ver las actividades sobre ese ganadero. Lo que registró antes conserva su autoría.

## 2. Operaciones idempotentes (`Idempotency-Key`)

En cualquier `POST` autenticado se puede enviar `Idempotency-Key: <valor único, hasta 100 caracteres>` (por ejemplo un UUID generado al encolar la operación).

- La primera petición se ejecuta y, si responde `2xx`, el resultado se guarda por usuario y clave.
- Repetir la misma petición con la misma clave devuelve el resultado guardado, con el encabezado `Idempotent-Replayed: true`, sin repetir el efecto.
- Misma clave con otra petición (método, ruta o cuerpo): `422`. Mientras la primera sigue en curso: `409` con `Retry-After`. Clave vacía o de más de 100 caracteres: `400`.
- Una respuesta de error (`4xx`/`5xx`) **no se guarda**: reintentar con la misma clave vuelve a ejecutarse, por ejemplo cuando el ganadero ya aceptó al veterinario.
- Las claves se conservan 24 horas. Sin el encabezado, nada cambia.

Reglas para el cliente: generar la clave **una sola vez por operación pendiente**, enviarla en cada reintento y descartarla cuando la operación termine con éxito o el usuario la descarte.

## 3. Versiones y conflictos (`ETag` / `If-Match`)

Animales, eventos sanitarios, actividades y registros financieros tienen `version` (empieza en 1 y sube en cada modificación). Las respuestas de `GET`, `POST`, `PUT` y `PATCH` la incluyen en el cuerpo (`version`) y en el encabezado `ETag: "3"`.

Para editar o borrar sin pisar cambios ajenos, el cliente envía `If-Match: "<versión en la que se basó>"` en `PUT`, `PATCH` y `DELETE`:

- Si coincide, el cambio se aplica y la versión sube.
- Si no coincide, el servidor responde `412 Precondition Failed` con `{ message, currentVersion, current }`, donde `current` es la copia del servidor, y **no cambia nada**. El cliente conserva ambas copias y aplica la regla de resolución:
  - **Conservar la del servidor:** descartar el cambio local.
  - **Conservar la mía:** reenviar el cambio con `If-Match: "<currentVersion>"`.
- Sin `If-Match` el comportamiento es el anterior (gana el último en escribir). `*` acepta cualquier versión.
- Si dos cambios compiten en el mismo instante, el perdedor recibe `409` y no se pierde nada.
