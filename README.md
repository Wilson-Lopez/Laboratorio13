# Veterinaria API (Semana 13)

Web API REST en capas con ASP.NET Core (.NET 10), Entity Framework Core 10 (base de datos primero), DTOs y AutoMapper para la gestión de **propietarios** y sus **mascotas** en una clínica veterinaria.

## Estructura de la solución

| Proyecto | Contenido | Referencia a |
|---|---|---|
| `Veterinaria.Entidades` | Entidades (`Propietario`, `Mascota`) y DTOs (`Dtos/`) | — |
| `Veterinaria.Datos` | `VeterinariaContext` (Scaffold) y repositorios con sus interfaces | Entidades |
| `Veterinaria.Negocio` | Servicios con sus interfaces, reglas de negocio, excepciones y `MappingProfile` | Datos |
| `Veterinaria.Api` | Controladores, `Program.cs`, pruebas `Veterinaria.Api.http` y panel web (`wwwroot`) | Negocio |

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

3. Ejecutar la API:

   ```
   dotnet run --project Veterinaria.Api --launch-profile http
   ```

   - Panel web que consume la API: `http://localhost:5169/`
   - Swagger: `http://localhost:5169/swagger`

4. Probar los endpoints en orden con `Veterinaria.Api/Veterinaria.Api.http` (volver a ejecutar el script SQL antes de repetir las pruebas).

## Panel web

`Veterinaria.Api/wwwroot` contiene un panel (HTML, CSS y JavaScript sin librerías) que consume la API con `fetch` desde el mismo sitio, por eso no necesita CORS. Usa solo los DTOs que devuelve la API: lista y filtra propietarios y mascotas con los GET por query string (con orden por columna y paginación en el navegador), muestra el contacto de un propietario con `GET api/propietarios/{id}` y sus mascotas con la ruta anidada, y registra, edita y elimina mostrando los errores 400 (por campo), 404 y 409 (regla de negocio) que responde la API. Como la API no guarda fotos, las mascotas se muestran con un dibujo según su especie y los propietarios con sus iniciales.

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
