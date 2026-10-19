# Trabajo práctico de Arquitecturas

## Incorporación de métodos de envío

Integrantes: completar con los nombres de los tres miembros del equipo.

El comercio electrónico dispone de productos, usuarios invitados, carritos y pedidos. Se incorpora el agregado ShippingMethod para representar las opciones de entrega ofrecidas por el comercio. Permite registrar una opción y recuperarla mediante su identificador, sin duplicar las entidades existentes.

El alcance es la administración básica del catálogo de métodos de envío: alta y consulta individual. La selección de un método en el checkout y su incorporación al total de un pedido quedan fuera de este trabajo. No se modificó el funcionamiento de los pedidos.

## Reglas de negocio

- El identificador es un GUID no vacío, generado por el servidor. El cliente no elige el identificador.
- El nombre es obligatorio y tiene hasta 100 caracteres después de quitar espacios de los extremos.
- El costo es un decimal mayor o igual a cero, con hasta dos decimales. Cero representa envío gratuito. Los importes usan la moneda del comercio; este alcance no incluye conversión de monedas.
- El plazo estimado es un número entero entre 1 y 60 días inclusive.
- Las propiedades tienen setters privados. La fábrica ShippingMethod.Create impide crear objetos inválidos, incluso si se llama sin pasar por HTTP.
- Los nombres no son únicos: pueden existir distintas opciones con el mismo nombre. El identificador distingue cada registro.

## Arquitectura

Se conserva el proyecto único y la organización por funcionalidades del código base. Las responsabilidades lógicas siguen separadas:

1. FastEndpoints recibe la solicitud y FluentValidation comprueba los datos.
2. El endpoint envía un comando o una consulta mediante Mediator.
3. El handler ejecuta el caso de uso. En el alta invoca la fábrica de dominio; en la consulta usa una especificación por identificador.
4. El handler depende de IRepository o IReadRepository del núcleo compartido, sin conocer EF Core ni SQLite.
5. EfRepository implementa esas abstracciones. AddAsync guarda los cambios.
6. AppDbContext y ShippingMethodConfiguration definen la persistencia y las conversiones de Vogen.

La respuesta usa ShippingMethodRecord: expone valores simples, sin publicar directamente la entidad de dominio. El dominio no depende de Infrastructure; se amplió la regla de NsDepCop para impedir que la nueva funcionalidad dependa de Infrastructure.

## Base de datos

Se agrega ShippingMethods con Id, Name, Cost y EstimatedDeliveryDays mediante una migración de EF Core, junto con su actualización del snapshot. Se mantiene SQLite y la migración inicial del profesor.

Se corrigió la fábrica de diseño para usar una cadena SQLite válida. RecreateOnStartup se dejó en false en desarrollo, de modo que los registros persistan entre ejecuciones. La aplicación aplica las migraciones pendientes al arrancar.

## Endpoints

| Método | Ruta | Resultado |
| --- | --- | --- |
| POST | /ShippingMethods | 201 Created con el objeto y cabecera Location |
| GET | /ShippingMethods/{id} | 200 OK con el objeto o 404 Not Found |

Ambos endpoints permiten acceso anónimo, siguiendo el estilo de los endpoints existentes del proyecto base.

### Crear

Solicitud:

```http
POST /ShippingMethods
Content-Type: application/json

{
  "name": "Envío local",
  "cost": 2500.50,
  "estimatedDeliveryDays": 2
}
```

Ejemplo de respuesta (el GUID real se genera en cada alta):

```http
HTTP/1.1 201 Created
Location: /ShippingMethods/5f68716d-c7c5-43bd-9a03-f133a32bb242
Content-Type: application/json

{
  "id": "5f68716d-c7c5-43bd-9a03-f133a32bb242",
  "name": "Envío local",
  "cost": 2500.50,
  "estimatedDeliveryDays": 2
}
```

Un nombre vacío, un costo negativo, un importe con más de dos decimales o un plazo fuera de rango devuelven 400. FastEndpoints genera los errores de validación; por ejemplo, al enviar name vacío:

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "name": ["El nombre es obligatorio."]
  }
}
```

Si se invoca el caso de uso sin HTTP, las reglas del dominio también se verifican y el handler devuelve Result.Invalid sin guardar el objeto.

### Consultar

```http
GET /ShippingMethods/5f68716d-c7c5-43bd-9a03-f133a32bb242
```

Devuelve 200 y el mismo objeto del ejemplo. Un GUID válido sin registro devuelve 404 sin cuerpo. Un identificador mal formado o el GUID vacío devuelve 400.

Los errores inesperados de infraestructura no se convierten en errores de validación. Se delegan al manejo global de excepciones del proyecto.

## Ejecutar

Requisito: SDK .NET 10. Abrir una terminal en la carpeta que contiene ISII.slnx.

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ISII.Web -- --urls http://localhost:5100
```

La primera ejecución crea la base SQLite mediante las migraciones. En desarrollo, abrir http://localhost:5100/scalar/v1 para probar los endpoints. El archivo ShippingMethods.http incluye solicitudes de ejemplo para Visual Studio, Rider o una extensión REST compatible.

No hace falta instalar SQL Server ni ejecutar una migración manualmente para iniciar la aplicación.

## Pruebas incluidas

ShippingMethodTests verifica dominio, límites válidos e inválidos, normalización del nombre, identificadores, validación de solicitudes y handlers. Usa NSubstitute para comprobar que el alta válida llama al repositorio y que una inválida no persiste. También verifica consultas existentes e inexistentes.

ShippingMethodApiTests usa WebApplicationFactory y una base SQLite temporal real, aislada de ISII.db. Comprueba POST, Location, GET, respuestas 400 y 404, envío gratuito, que los intentos inválidos no agreguen filas y persistencia después de reiniciar el host. Al iniciar usa las migraciones, no EnsureCreated.

Las pruebas ProductTest del profesor se conservan.

## Ajustes de soporte

Se actualizó explícitamente System.Security.Cryptography.Pkcs a 10.0.13 porque la dependencia transitiva 10.0.0 del proyecto base provocaba NU1902 y detenía la compilación al tratar advertencias como errores. No se deshabilitó la auditoría.

## Resultado de verificación

Se ejecutó la solución con .NET 10: compilación correcta y 37 pruebas aprobadas (35 pruebas de dominio, validación y aplicación, incluidas las 7 originales de productos; y 2 pruebas de integración HTTP con SQLite). No hubo pruebas fallidas ni omitidas en la ejecución final.

Las pruebas de integración confirmaron creación, consulta, errores, aplicación de migraciones y conservación de los datos al reiniciar el host usando el mismo archivo SQLite.
