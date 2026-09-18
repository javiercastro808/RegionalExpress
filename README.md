# Regional Express — proyecto grupal

Aplicación de envíos y pedidos de restaurantes con paneles de administración, clientes y motoristas, asignaciones y rastreo de ubicación.

## Tecnología

- Frontend: Angular 22, Bootstrap y Leaflet.
- API: ASP.NET Core .NET 10 y Entity Framework Core.
- Base de datos: SQL Server; el entorno local utiliza LocalDB.

## Ejecutar en desarrollo

En el PC que ya tiene la configuración y base de datos, cerrar las ejecuciones antiguas y ejecutar `./scripts/Iniciar-Pruebas.ps1` desde la raíz. Abrir http://localhost:4300. La página y API se ejecutan juntas para las pruebas; el servidor interno usa 5292. Para compartir el enlace HTTPS, ver `docs/PUBLICACION.md`.

Requisitos: Node compatible con Angular 22, SDK .NET 10 y SQL Server con la base RegionalExpressDB existente.

1. En `backend/RegionalExpress.API`, copiar `appsettings.Example.json` como `appsettings.json` **solo si no existe**. Configurar conexión y una clave JWT aleatoria privada. No utilizar el texto de ejemplo como clave.
2. En esa carpeta ejecutar `dotnet run --urls http://localhost:5192`.
3. En otra terminal, desde `frontend`, ejecutar `npm ci` y `npm start -- --port 4201`.
4. Abrir http://localhost:4201.

La base de datos y las cuentas locales no se incluyen en Git. `database/Esquema.sql` contiene la estructura; el comando `--inicializar-demo` prepara una base vacía con datos ficticios. Consultar `docs/PUBLICACION.md` antes de usarlo. No ejecutar el esquema sobre una base existente.

## Verificación

Desde `frontend`: `npm run check` y `npm run build`.
Desde `backend/RegionalExpress.API`: `dotnet build`.

Prueba funcional: ingresar por rol, crear servicio, asignar motorista, cambiar estado, compartir ubicación y rastrear por código. DELIVERY con modalidad RECOGER y servicios finalizados no deben admitir asignación.

El motorista debe permitir la ubicación y mantener abierta su página. Para geolocalización fuera de localhost se requiere HTTPS. Las fotografías generadas del menú se identifican como imágenes de referencia.

## GitHub y alojamiento

Nombre propuesto del repositorio privado: `proyecto-grupal-regional-express`.
Las configuraciones privadas, correos locales, dependencias y respaldos están excluidos mediante `.gitignore`.

Repositorio: https://github.com/javiercastro808/proyecto-grupal-regional-express

Para preparar los archivos publicables ejecutar `./scripts/Preparar-Publicacion.ps1` desde PowerShell. El resultado es un ZIP en `artifacts/`, con Angular y API juntos y sin configuraciones privadas. Para subir el código ejecutar `./scripts/Subir-GitHub.ps1`.

Consultar [el plan de publicación](docs/PUBLICACION.md), que distingue lo implementado de las verificaciones y accesos pendientes.
