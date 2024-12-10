# API de gestión de inventario
Esta API es una solución para la gestión de inventarios, construida con ASP.NET Core, Entity Framework Core, SQL Server y .NET 8.0. Permite realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre los productos en inventario, gestionar categorías, y manejar autenticación mediante JWT. Además, esta API integra FluentValidation para la validación de datos de entrada y EPPlus para la manipulación de archivos Excel.

# Tecnologias utilizadas
ASP.NET Core: Framework para desarrollar la API.
Entity Framework Core: ORM para interactuar con la base de datos.
SQL Server: Base de datos utilizada para almacenar los datos del inventario.
.NET 8.0: Versión del framework de desarrollo.
FluentValidation: Para la validación de datos de entrada.
JwtBearer: Para la autenticación mediante JWT.
EPPlus: Para la manipulación de archivos Excel, especialmente para la importación/exportación de inventarios.

# Instalación
- Requisitos previos:
  .NET 8.0 SDK: Asegúrate de tener instalada la versión 8.0 del SDK de .NET en tu máquina.
  SQL Server: Necesitas tener una instancia de SQL Server en funcionamiento o usar SQL Server Express.

- Pasos para ejecutar el proyecto
Clona este repositorio:
git clone https://github.com/Kilver15/InventarioAPI.git

Restaura las dependencias: Navega al directorio del proyecto y ejecuta:
dotnet restore

Configura la cadena de conexión:
Abre el archivo appsettings.json y asegúrate de configurar la cadena de conexión a tu base de datos de SQL Server:
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventarioDB;Trusted_Connection=True;"
}

Ejecuta las migraciones de la base de datos: 
Si es la primera vez que ejecutas el proyecto, ejecuta las migraciones para crear la base de datos:
dotnet ef database update

Ejecuta la API: 
Finalmente, para ejecutar el proyecto, usa:
dotnet run

# Dependencias
La API utiliza las siguientes dependencias:

Microsoft.EntityFrameworkCore: ORM para interactuar con la base de datos.
Microsoft.EntityFrameworkCore.Tools: Herramientas para trabajar con migraciones y la base de datos.
Microsoft.EntityFrameworkCore.SqlServer: Proveedor de SQL Server para Entity Framework Core.
FluentValidation.DepencyInjectionExtensions: Para la integración de validación de datos mediante FluentValidation.
SharpGrip.FluentValidation.AutoValidation.Mvc: Integración automática de validación con MVC.
Microsoft.AspNetCore.Authentication.JwtBearer (Versión 8.0.11): Autenticación JWT para la API.
EPPlus: Biblioteca para la manipulación de archivos Excel, utilizada en la importación y exportación de inventarios.

# Endpoints
La documentación completa de los endpoints está disponible aquí:
https://www.postman.com/joint-operations-participant-81264458/inventario-api/collection/xx2excs/inventarioapi?action=share&creator=39943883


