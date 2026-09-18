# Publicación gratuita de Regional Express

## Modalidad elegida: GitHub y PC como servidor

Se pospone el alojamiento externo. El código se comparte por GitHub y las pruebas se ejecutan en el PC del desarrollador.

1. Cerrar con Ctrl+C las terminales antiguas que ejecutaban Regional Express.
2. Desde la raíz ejecutar `./scripts/Iniciar-Pruebas.ps1`. Levanta la API en 5292 y Angular en 4300; abrir http://localhost:4300. Mantener la terminal abierta.
3. Para compartir fuera del PC: en VS Code abrir la vista Puertos, seleccionar Reenviar un puerto, escribir 4300 e iniciar sesión en GitHub si se solicita. Copiar la dirección HTTPS.
4. El enlace es privado inicialmente. Para que los compañeros puedan acceder sin tu cuenta GitHub, cambiar Visibilidad del puerto a Público. Cualquier persona con el enlace podrá abrir la página; los paneles y los datos privados siguen requiriendo una cuenta autorizada de Regional Express. Utilizar datos ficticios en las pruebas.
5. Al cerrar la terminal o apagar el PC deja de funcionar el enlace. Detener también el reenvío desde la vista Puertos al terminar.

Guía oficial: https://code.visualstudio.com/docs/debugtest/port-forwarding

Las alternativas de alojamiento permanente siguientes quedan como referencia, no como tareas necesarias para continuar.

## Estado verificable

Repositorio privado creado: https://github.com/javiercastro808/proyecto-grupal-regional-express.
La subida del código y el despliegue no se consideran terminados hasta ver los archivos en GitHub y probar la URL del alojamiento.

Implementado: hashes de contraseñas y migración local compatible; validación del usuario activo; cliente obtenido del token; precios de pedidos calculados en servidor; historial del cliente; rastreo privado de coordenadas; códigos nuevos aleatorios largos; orígenes CORS explícitos; frontend y API preparados para un solo dominio. Los códigos antiguos siguen funcionando.

## Subir a GitHub

Desde PowerShell en la raíz del proyecto:

```powershell
./scripts/Subir-GitHub.ps1
```

El script solicita nombre/correo de autor si Git aún no los tiene, excluye archivos privados, crea la versión y envía a `main`. Completar el inicio de sesión de Git si se solicita. No pegar contraseñas ni tokens en el chat.

## Preparar el paquete

```powershell
./scripts/Preparar-Publicacion.ps1
```

Requiere Node compatible con Angular 22 y SDK .NET 10. Comprueba Angular, prueba contraseñas, compila ambos proyectos y genera `artifacts/publicacion-FECHA.zip`. El ZIP no incluye `appsettings`, datos de clientes ni correos locales. Se necesita añadir la configuración privada en el alojamiento.

También hay una acción manual de GitHub `Verificar Regional Express` para generar el paquete después de subir el código. Usarla dentro de la cuota gratuita de Actions; no habilitar cobros. No se ejecuta automáticamente con cada cambio.

## Alojamiento sin costo

Se evaluó MonsterASP.NET por ofrecer .NET 10 y SQL Server sin tarjeta. Su tabla actual indica 1 sitio, 1 base de 1 GB y 256 MB de RAM, con límites de tráfico. Hay una discrepancia sobre HTTPS: la guía explica activarlo gratis con renovación manual, mientras la tabla de precios lo excluye. Confirmar en el panel que HTTPS está disponible a costo cero ANTES de publicar una aplicación con inicio de sesión y GPS. No activar planes Premium ni publicar credenciales sobre HTTP.

Si el plan gratuito no permite HTTPS, evaluar Azure for Students (requiere elegibilidad y cuenta) con recursos gratuitos y suspensión al agotar cuotas. No contratar recursos de pago. Todavía no hay una URL permanente verificada.

Fuentes:
- https://www.monsterasp.net/Pricing/
- https://help.monsterasp.net/books/https/page/how-to-activate-https-with-lets-encrypt-certificate
- https://azure.microsoft.com/en-us/free/students

## Configuración privada del alojamiento

Configurar `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection`, `Jwt__Key` (aleatoria de al menos 32 bytes), `Jwt__Issuer=RegionalExpress`, `Jwt__Audience=RegionalExpress` y `Correo__Modo=Desactivado` hasta tener SMTP real. Usar conexión SQL cifrada y el servidor/base/usuario proporcionados por el alojamiento. No usar LocalDB allí. No subir `appsettings.Example.json` como configuración real.

La publicación incluye Angular en `wwwroot` y la API en `/api`. Probar `/api/salud`, login, enlaces directos y refrescar páginas. Activar HTTPS y redirección a HTTPS en el panel del alojamiento.

## Base nueva para compañeros

Usar una base SQL vacía y dedicada a pruebas. La estructura está en `database/Esquema.sql`, generada del modelo, sin datos personales. Puede crearse la estructura manualmente o permitir que la inicialización cree tablas en una base vacía.

Configurar privadamente cuatro valores diferentes de al menos 12 caracteres: `Demo__AdminPassword`, `Demo__RestaurantePassword`, `Demo__MotoristaPassword` y `Demo__ClientePassword`. No publicarlos en GitHub. Desde el proyecto API, con la conexión apuntando a la base de pruebas:

```powershell
dotnet run -- --inicializar-demo
```

La operación se niega a modificar una base con usuarios, restaurantes, roles o estados existentes. Crea cuatro cuentas ficticias: `admin@regionalexpress.test`, `restaurante@regionalexpress.test`, `motorista@regionalexpress.test` y `cliente@regionalexpress.test`. Sus claves son las configuradas privadamente. Incluye restaurante, menú de cuatro productos, motorista disponible y estados por modalidad. No añade pedidos ni posiciones GPS falsos. Retirar las variables Demo tras inicializar.

## Conservar las cuentas locales

No inicializar la base local existente. Reiniciar primero el servidor desde esta carpeta nueva. Las cuentas antiguas migran su contraseña al entrar en Development, manteniendo la misma clave. Para convertirlas todas explícitamente antes de usar una copia autorizada en Production, respaldar la base y ejecutar:

```powershell
dotnet run -- --migrar-passwords
```

Este comando cambia el almacenamiento a hash; no cambia la contraseña que escribe cada usuario. No volver al servidor antiguo que comparaba texto plano después de migrar.

## Verificación y límites

Se comprobó TypeScript/plantillas, compilación C# con dependencias locales, ocho casos de contraseñas y las pruebas existentes de sesión/rutas/imágenes/GPS. La compilación estándar completa y la conexión SQL deben verificarse en una terminal con acceso normal: el entorno del asistente bloqueó NuGet.Config y el escaneo del directorio padre de Angular incluso tras solicitar permiso.

Seguir `docs/PRUEBAS.md` para pruebas de extremo a extremo. Pendientes: URL HTTPS gratuita confirmada, publicación efectiva, pruebas con varias cuentas contra SQL y GPS real, proveedor SMTP/recuperación por correo, y cálculo de distancia por rutas. Las fotografías del menú son referencias, no fotografías reales del restaurante.
