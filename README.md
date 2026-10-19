# TP de Arquitecturas — Métodos de envío

Integrantes: Baigorria Juan, Rodriguez Daniel, Ponce Tomás.

## Objeto agregado

Se agregó `ShippingMethod` al comercio electrónico para registrar métodos de envío y consultarlos por su identificador. Tiene nombre, costo y plazo estimado de entrega. Los datos se guardan en SQLite mediante Entity Framework Core, respetando la organización del proyecto base.

## Reglas de negocio

- **ID:** GUID no vacío, generado automáticamente por el sistema.
- **Nombre:** obligatorio, hasta 100 caracteres; se eliminan los espacios de los extremos.
- **Costo:** mayor o igual a cero, con hasta dos decimales. Cero permite envío gratuito.
- **Plazo estimado:** entre 1 y 60 días inclusive.

## Endpoints

| Método | Ruta | Respuestas |
| --- | --- | --- |
| POST | `/ShippingMethods` | 201 si se crea; 400 si los datos son inválidos |
| GET | `/ShippingMethods/{id}` | 200 si existe; 404 si no existe; 400 si el ID es inválido |

Ejemplo de solicitud para crear:

```http
POST /ShippingMethods
Content-Type: application/json

{
  "name": "Envío local",
  "cost": 2500.50,
  "estimatedDeliveryDays": 2
}
```

Ejemplo de respuesta **201 Created**:

```json
{
  "id": "5f68716d-c7c5-43bd-9a03-f133a32bb242",
  "name": "Envío local",
  "cost": 2500.50,
  "estimatedDeliveryDays": 2
}
```

La cabecera `Location` indica la ruta para consultar el objeto. Una solicitud `GET /ShippingMethods/5f68716d-c7c5-43bd-9a03-f133a32bb242` devuelve **200 OK** con el mismo JSON si existe ese registro.

## Pruebas

Se verificaron las reglas del dominio, la validación de datos y los casos de uso de creación y consulta. También se probó la API con SQLite, los errores y la conservación de datos al reiniciar. La ejecución completa aprobó **37 pruebas**, incluidas las originales del proyecto.

## Ejecución

Requiere SDK .NET 10. Desde la carpeta principal:

```powershell
dotnet test
dotnet run --project src/ISII.Web -- --urls http://localhost:5100
```

La aplicación aplica las migraciones al iniciar. En desarrollo, la documentación interactiva está en http://localhost:5100/scalar/v1.

Proyecto base: https://github.com/alexiswalker/clean_architecture
