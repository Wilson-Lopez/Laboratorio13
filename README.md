# Veterinaria API (Semana 13)

Web API REST en capas con ASP.NET Core (.NET 10), Entity Framework Core 10 (base de datos primero), DTOs y AutoMapper para la gestión de **propietarios** y sus **mascotas** en una clínica veterinaria.

## Estructura de la solución

| Proyecto | Contenido | Referencia a |
|---|---|---|
| `Veterinaria.Entidades` | Entidades (`Propietario`, `Mascota`) y DTOs (`Dtos/`) | — |
| `Veterinaria.Datos` | `VeterinariaContext` (Scaffold) y repositorios con sus interfaces | Entidades |
| `Veterinaria.Negocio` | Servicios con sus interfaces, reglas de negocio, excepciones y `MappingProfile` | Datos |
| `Veterinaria.Api` | Controladores, `Program.cs` y pruebas `Veterinaria.Api.http` | Negocio |

Relación: un propietario (1) tiene muchas mascotas (N), mediante la clave foránea `Mascotas.PropietarioId`.

## Reglas de negocio (409 Conflict)

1. No se puede registrar un propietario con un DNI que ya existe.
2. No se puede eliminar un propietario que tiene mascotas registradas.
3. Un propietario no puede tener dos mascotas con el mismo nombre (al crear o actualizar).

## Cómo ejecutarla

1. Crear la base de datos con sus datos de ejemplo:

   ```
   sqlcmd -S .\SQLEXPRESS -E -C -f 65001 -i VeterinariaDB.sql
   ```

2. Guardar la cadena de conexión en user-secrets (no se sube al repositorio):

   ```
   dotnet user-secrets set "ConnectionStrings:VeterinariaDB" "Server=.\SQLEXPRESS;Database=VeterinariaDB;Trusted_Connection=True;TrustServerCertificate=True" --project Veterinaria.Api
   ```

3. Ejecutar la API y abrir Swagger en `http://localhost:5169/swagger`:

   ```
   dotnet run --project Veterinaria.Api --launch-profile http
   ```

4. Probar los endpoints en orden con `Veterinaria.Api/Veterinaria.Api.http` (volver a ejecutar el script SQL antes de repetir las pruebas).

## Endpoints

| Método | Ruta | Códigos |
|---|---|---|
| GET | `api/propietarios?nombres=&dni=` | 200 |
| GET | `api/propietarios/{id}` | 200, 404 |
| GET | `api/propietarios/{id}/mascotas` | 200, 404 |
| POST | `api/propietarios` | 201, 400, 409 |
| PUT | `api/propietarios/{id}` | 204, 400, 404 |
| DELETE | `api/propietarios/{id}` | 204, 404, 409 |
| GET | `api/mascotas?especie=&raza=&nombre=` | 200 |
| GET | `api/mascotas/{id}` | 200, 404 |
| POST | `api/mascotas` | 201, 400, 409 |
| PUT | `api/mascotas/{id}` | 204, 400, 404, 409 |
| DELETE | `api/mascotas/{id}` | 204, 404 |
