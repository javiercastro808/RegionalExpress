# Manual práctico de Regional Express

Complemento del informe DERCAS · Versión 0.1 · CP-001 · 30 de septiembre de 2026

Este tutorial describe la versión consolidada en `4e188e8`. Se dirige al equipo que desarrolla y prueba el proyecto. Cada procedimiento indica qué hacer, qué debe ocurrir y cómo registrar un problema. No sustituye la instalación validada en otra computadora, que sigue pendiente.

## 1 Preparar el entorno existente

Requisitos del proyecto: VS Code, Node compatible con Angular del repositorio, SDK .NET 10 y SQL Server/LocalDB con la base RegionalExpressDB configurada. Las versiones de dependencias se encuentran en frontend/package.json y los archivos del backend. No copiar configuraciones privadas a GitHub.

En PowerShell, entrar en la carpeta canónica:

```powershell
cd "C:\Users\polo-\Documents\My Web Sites\RegionalExpress"
git status
```

**Problema que evita:** trabajar en la copia antigua de Videos o reemplazar correcciones recientes con un ZIP anterior. Antes de continuar, comprobar que se está en el repositorio correcto y revisar cambios existentes.

## 2 Iniciar la aplicación

```powershell
.\scripts\Iniciar-Pruebas.ps1
```

El script compila la API, prepara dependencias si faltan, inicia el servidor interno y ejecuta Angular con la configuración de pruebas. Abrir http://localhost:4300 y mantener la terminal abierta. Para detenerlo, usar Ctrl+C; si PowerShell pregunta si desea terminar el trabajo por lotes, confirmar.

**Resultado esperado:** API disponible, Angular iniciado y página accesible. No se necesita abrir SQL Server Management Studio para usar la aplicación, pero el motor y la base configurada sí deben estar disponibles.

**Si falla:** guardar el error completo sin claves ni tokens. INC-009 documenta el comando start:pruebas ausente y su corrección. Un puerto ocupado requiere cerrar la ejecución anterior; un error SQL requiere revisar la configuración local, no volver a generar toda la base.

## 3 Entender la implementación del carrito

**Problemática:** al cambiar Domicilio/Recoger el envío se acumulaba y algunos cambios de TypeScript rompían la plantilla.

**Implementación:** services/carrito.ts mantiene productos y cantidades; pages/restaurante conserva el contrato con su HTML y calcula subtotal, envío y total desde el estado actual. El envío no se suma sobre el total anterior. Los precios finales también se comprueban en servidor.

**Prueba guiada:** iniciar sesión como CLIENTE, entrar a un restaurante, añadir dos productos distintos y subir uno a cantidad dos. Anotar el subtotal y alternar la modalidad al menos cinco veces. En domicilio debe mantenerse subtotal + Q10; en recoger debe mantenerse subtotal + Q0. Un carrito vacío no debe cobrar envío. Un producto de otro restaurante debe ser rechazado hasta vaciar el carrito anterior.

**Evidencia:** registrar cantidades, subtotal y total sin datos personales. INC-001 e INC-002 contienen el problema original. Existe confirmación del usuario sobre los totales; repetir la prueba si se modifica el cálculo.

## 4 Probar las sesiones y los roles

Abrir pestañas independientes e ingresar con las cuentas correspondientes. La sesión se almacena por pestaña; cada carrito corresponde a la cuenta de esa pestaña. Para una prueba inequívoca, abrir una pestaña nueva y escribir la dirección, en lugar de asumir que duplicar una pestaña siempre comienza sin sesión.

- CLIENTE solicita servicios y compra.
- ADMIN_GENERAL administra y asigna; no compra con esa misma cuenta.
- ADMIN_RESTAURANTE gestiona su restaurante y los estados permitidos.
- MOTORISTA consulta su asignación y confirma pasos.

**Prueba:** cerrar sesión en una pestaña, recargar otra y comprobar que conserva su usuario. La separación visual no sustituye los controles JWT y de roles del backend. Registrar cualquier acceso incorrecto como incidencia de autorización.

## 5 Crear usuarios y un perfil de motorista

En el panel general abrir Usuarios → Nuevo usuario. Completar nombre, correo válido, rol y contraseña de al menos ocho caracteres. Al editar, dejar la contraseña vacía conserva la actual. Para dar de alta un motorista, crear primero la cuenta con rol MOTORISTA y después asociarla en la sección Motoristas, donde aparecen usuarios elegibles sin perfil.

**Problemática corregida:** el alta de usuario omitía Apellido, obligatorio en el modelo persistido, y el error era genérico. AdminController inicializa el campo y fecha; frontend y API validan entradas y el formulario presenta el error cerca de los controles.

**Estado de validación:** la creación con el script de Motorista 2 está confirmada. Las altas desde el panel después de INC-015 deben probarse contra SQL. No usar la creación por script como prueba de que funciona el formulario, porque son rutas distintas.

## 6 Probar asignación automática y control administrativo

Para comida a domicilio, crear el pedido y avanzar desde el restaurante hasta un estado elegible de pedido listo. Mantener un motorista activo, disponible y sin otro servicio activo. El servidor revisa la asignación cada 15 segundos; la pantalla del motorista consulta novedades cada 20 segundos. Puede haber demora entre ambos ciclos.

Para paquetes, completar origen y destino y registrar el envío. Los servicios elegibles sin motorista esperan si no existe un candidato libre. Los pedidos para recoger están excluidos.

**Intervención manual:** el administrador puede retirar la asignación y luego escoger otro motorista disponible. Tras retirar, el servicio queda bajo control manual; el asignador no debe recuperarlo automáticamente. La elección no se basa en distancia GPS ni disponibilidad en línea: emplea disponibilidad registrada y carga histórica.

**Implementación:** AsignadorAutomatico procesa candidatos, ReglasAsignacion determina elegibilidad y AsignacionesController mantiene las operaciones administrativas. Se utilizan transacciones para coordinar cambios; su comportamiento concurrente requiere prueba real, no solo compilación.

**Validación pendiente:** usar dos motoristas y dos servicios, comprobar que no se ocupa al mismo motorista con ambos y registrar qué sucede al entregar, retirar y reasignar.

## 7 Compartir ubicación y confirmar pasos

En el teléfono del motorista abrir el enlace HTTPS, ingresar con su cuenta y abrir el servicio. Permitir ubicación y esperar el mensaje de confirmación del servidor. Pulsar el botón del siguiente paso únicamente después de realizar esa acción. No confirmar una entrega que no haya ocurrido durante una prueba real.

**Diferencias importantes:** permiso concedido significa que el navegador puede solicitar coordenadas; posición recibida significa que el dispositivo respondió; confirmada significa que el servidor guardó la ubicación. Ninguna de esas etapas debe inferirse de las otras.

La regla actual acepta coordenadas recientes hasta 500 m de precisión, con aviso de aproximación entre 150 y 500 m. Por encima del límite se informa y se reintenta. La prueba automática cubre los ejemplos de 245 y 501 m. El servidor exige ubicación reciente del servicio asignado; no se inventan puntos para habilitar el avance.

Si la computadora no entrega ubicación, probar con el teléfono y GPS activo. Mantener la página abierta. El sistema web no garantiza seguimiento en segundo plano ni con pantalla bloqueada.

## 8 Consultar y ampliar el mapa

En Rastreo introducir el código del servicio e ingresar con una cuenta autorizada para ver sus datos privados. El mapa presenta origen azul, cliente/destino rojo y motorista verde. Si el motorista comparte después, el mapa debe reajustarse para incluirlo.

La consulta abre el mapa con aproximadamente 75% del alto visible. Usar Reducir mapa o Ampliar mapa para cambiar el tamaño, y Ver todos los puntos para encuadrar marcadores. En los formularios de selección de ubicación también existe ampliación, hasta aproximadamente 70% del alto visible.

**Problemática:** la vista podía permanecer centrada en el destino y ocultar visualmente al motorista lejano. **Implementación:** puntos separados, etiquetas, reencuadre y cambio de tamaño. **Pendiente:** captura de aceptación desde dos ubicaciones reales, anonimizando los puntos antes de incluirla en el informe. Los marcadores no son una ruta calculada por carretera.

## 9 Compartir pruebas con compañeros

Con el proyecto ejecutándose, usar la vista Puertos de VS Code para reenviar 4300 y compartir el enlace HTTPS. Si se habilita visibilidad pública, quien tenga el enlace puede abrir la página, aunque los paneles privados siguen requiriendo autenticación. Usar datos de prueba y detener el reenvío al terminar.

Los compañeros utilizan la misma API y base de datos de la computadora anfitriona. Subir a GitHub comparte código, no inicia un servidor ni copia SQL. Para instalar en otra PC deben seguirse los requisitos y configuración del proyecto; la prueba de portabilidad permanece pendiente.

## 10 Ejecutar comprobaciones y registrar resultados

Desde frontend:

```powershell
npm.cmd run check
npm.cmd run test:logic
```

Desde backend/RegionalExpress.API:

```powershell
dotnet build --no-restore
```

Desde la raíz, para las comprobaciones backend:

```powershell
dotnet run --project tests/Seguridad/Seguridad.csproj --no-restore
```

Registrar fecha, commit, comando, resultado y limitaciones. No reemplazar el resultado esperado por el obtenido. Un resultado de compilación correcto no demuestra por sí solo conexión SQL, calidad del mapa o aceptación del usuario.

## 11 Guardar avances y generar el próximo checkpoint

Revisar git status y el contenido modificado. Añadir a BITACORA.md las incidencias nuevas o la evidencia de cierre. Añadir a CHECKPOINTS.md el siguiente corte cuando exista una etapa validada, usando el commit real. El script scripts/Subir-GitHub.ps1 puede guardar y subir los archivos del proyecto después de revisar su alcance.

El CP-001 no es un respaldo completo del sistema: Git conserva el código y documentos versionados, pero la base y configuración privada necesitan su propio procedimiento de respaldo y restauración. No publicar esos datos como anexos.

## 12 Formato de una entrada del manual DERCAS

1. **Problemática:** situación inicial, usuario afectado y pasos que producen el error.
2. **Análisis:** causa verificada y alternativas consideradas; marcar lo que aún es hipótesis.
3. **Implementación:** archivos o módulos modificados y comportamiento resultante.
4. **Tutorial:** pasos para usar o reproducir la solución.
5. **Comprobación:** evidencia, prueba, resultado obtenido y persona que validó.
6. **Conclusión y pendientes:** qué quedó resuelto y qué falta comprobar.
7. **Trazabilidad:** incidencia, fecha y commit/checkpoint asociado.

Aplicar esta estructura a cada nueva corrección. Las 22 incidencias actuales son el punto de partida, no 22 cierres definitivos: consultar el estado de cada una.
