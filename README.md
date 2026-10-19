# TP Arquitecturas — Métodos de envío

Extensión del proyecto base del profesor con el objeto ShippingMethod.

## Contenido

- Entidad de dominio con identificador generado, nombre, costo y plazo estimado.
- Validación del nombre obligatorio de hasta 100 caracteres, costo no negativo con hasta dos decimales y plazo de 1 a 60 días.
- Persistencia SQLite y migración EF Core.
- POST /ShippingMethods y GET /ShippingMethods/{id}.
- Pruebas unitarias y de integración.
- ENTREGA.md con descripción del objeto, reglas, endpoints, ejemplos y pruebas.

## Abrir y ejecutar

Esta carpeta contiene el proyecto completo. Abrir ISII.slnx o una terminal en esta carpeta. Requisito: SDK .NET 10.

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ISII.Web -- --urls http://localhost:5100
```

Con la aplicación iniciada, abrir http://localhost:5100/scalar/v1 para probar los endpoints. La aplicación aplica automáticamente las migraciones al iniciar.

El código de dominio, los endpoints y las pruebas están escritos en C#.

## Entrega

Completar los nombres de los integrantes en ENTREGA.md.

Proyecto base: https://github.com/alexiswalker/clean_architecture
