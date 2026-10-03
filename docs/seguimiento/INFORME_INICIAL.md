# Regional Express
## Manual DERCAS de desarrollo y resolución de problemas

Versión documental: 0.1 · Corte: 30 de septiembre de 2026
Checkpoint: CP-001 · Código base: `4e188e845ecd11ea9d39abf1d83ad08524e57ec4`
Integrantes: por completar · Curso y docente: por completar

**Formato DERCAS acordado con el usuario.** En este proyecto se utiliza como un manual o tutorial que documenta la problemática, su análisis, la implementación, los pasos de uso y la comprobación de resultados. Esta estructura sigue esa explicación; no se le atribuye una expansión de siglas ni una norma externa.

## 1 Resumen del proyecto

Regional Express es una plataforma web académica para pedidos de comida, envíos de paquetes y rastreo de servicios. Integra la solicitud del cliente, la gestión del restaurante, la asignación de motoristas y el seguimiento de estados y ubicaciones. El trabajo realizado ha consistido en estabilizar un proyecto existente, corregir problemas reportados en pruebas y completar funciones del flujo operativo.

El checkpoint CP-001 registra el código consolidado y subido a GitHub. Existe evidencia de funcionamiento del carrito, de obtención de ubicación desde un teléfono y de creación de una segunda cuenta de motorista. También existen verificaciones de código y pruebas automatizadas. No se ha documentado todavía una prueba completa de aceptación con todos los roles, dos motoristas y servicios simultáneos; por ello el proyecto no se declara finalizado ni listo para producción.

## 2 Antecedentes y problema abordado

En la conversación inicial se planteó una aplicación con cuatro roles y un ciclo completo de servicio. Durante la integración aparecieron incompatibilidades entre componentes Angular y sus plantillas, cobros de envío acumulados, interferencias entre sesiones y diferencias entre la asignación almacenada y la mostrada en pantalla. Las pruebas compartidas posteriormente revelaron dificultades con GPS, mensajes ocultos, encuadre del mapa, altas de usuarios y diseño en móvil.

También coexistían dos carpetas del proyecto y un ZIP anterior. Esto generaba riesgo de trabajar sobre una copia desactualizada o reemplazar funciones ya corregidas. Se estableció como carpeta de trabajo `C:\Users\polo-\Documents\My Web Sites\RegionalExpress` y se preservó el repositorio existente.

## 3 Objetivos

**General.** Desarrollar y validar una plataforma que permita gestionar pedidos y envíos desde su solicitud hasta la entrega, con acceso por roles y trazabilidad del servicio.

**Específicos.** Mantener cálculos correctos de cantidades y totales; separar las sesiones de prueba; registrar ubicaciones necesarias; facilitar la confirmación de pasos del motorista; permitir asignación automática con intervención administrativa; presentar información legible en computadora y teléfono; y documentar cambios, pruebas, incidentes y pendientes mediante checkpoints.

## 4 Alcance y arquitectura

| Elemento | Alcance del proyecto |
| --- | --- |
| Cliente | Consultar catálogo, gestionar carrito, pedir a domicilio o recoger, solicitar envíos y rastrear |
| Administrador general | Gestionar usuarios, restaurantes, motoristas y asignaciones |
| Administrador de restaurante | Gestionar catálogo, disponibilidad, pedidos y estados permitidos |
| Motorista | Consultar servicios, compartir ubicación y confirmar el siguiente paso |
| Frontend | Angular; rutas y servicios existentes conservados |
| Backend | ASP.NET Core Web API sobre .NET 10 |
| Persistencia | SQL Server mediante Entity Framework Core |
| Acceso | JWT, roles y contraseñas almacenadas con hash |
| Mapas | Leaflet con cartografía de OpenStreetMap |
| Versionado | Git y repositorio RegionalExpress en GitHub |

Flujo general: cliente → pedido o envío → código de rastreo → gestión/asignación → motorista → estados e historial → entrega. En pedidos a domicilio, la asignación automática espera un estado elegible de pedido listo; los pedidos para recoger no requieren motorista.

## 5 Método de trabajo y registro

Las correcciones siguieron un ciclo de observación del problema, revisión del código, implementación, comprobación y devolución al usuario. Las capturas y salidas de terminal permitieron identificar condiciones que no aparecían en las pruebas de lógica. Cada incidencia de la bitácora incluye síntoma, causa o hipótesis, cambio, evidencia y condición de cierre.

La documentación se reconstruye retrospectivamente a partir de la conversación, los archivos del repositorio y las salidas suministradas. Cuando no existe fecha exacta del incidente se usa el orden de trabajo y no una fecha inventada. Los commits son los hitos verificables de versionado; no se presentan como fechas individuales de todas las correcciones.

## 6 Desarrollo y decisiones principales

### Cálculos del carrito

Se mantuvieron los nombres utilizados por las plantillas para evitar nuevas incompatibilidades. Se organizaron los importes por ítem, subtotal general y total; el envío se calcula según la modalidad, Q10 para domicilio y Q0 para recoger, sin acumularse al alternar. Se incorporaron cálculos en centavos y restricciones para evitar mezclar restaurantes. La prueba de lógica contempla 100 alternancias; el usuario confirmó posteriormente que la página y los totales funcionaban correctamente.

### Sesiones y permisos

Se separó la sesión por pestaña y el carrito por cuenta y pestaña, conservando las verificaciones de autorización en servidor. Las cuentas administrativas no se convierten en clientes: la interfaz de gestión informa que las compras requieren una cuenta CLIENTE. Esto resuelve una confusión de navegación sin ampliar los permisos de compra del administrador.

### Ubicación y seguimiento

El cliente debe indicar destino para entrega a domicilio, con GPS o selección de punto. Los envíos requieren origen y destino. El motorista necesita coordenadas recientes y confirmación del servidor para avanzar. El permiso del navegador no garantiza que el dispositivo entregue una posición: se añadieron tiempo máximo de espera, reintentos y mensajes diferenciados.

Las primeras pruebas exigían precisión de 150 m. Tras el reporte de una posición rechazada de 245 m, se aceptaron posiciones hasta 500 m y se añadió aviso de ubicación aproximada entre 150 y 500 m. Este cambio facilita la prueba en dispositivos con menor precisión, pero no garantiza exactitud de domicilio ni navegación por calles. El rastreo mantiene origen, destino y motorista separados, reencuadra cuando aparece un nuevo tipo de punto y muestra el mapa ampliado al consultar.

### Operación del motorista y asignaciones

Se sustituyó la selección libre de estados por confirmación del siguiente paso, con rechazo de saltos y retrocesos en la API. El asignador consulta cada 15 segundos y selecciona un motorista elegible, disponible y sin servicio activo, priorizando menor cantidad histórica de servicios. El panel del motorista consulta novedades cada 20 segundos; no se describe como notificación instantánea.

El administrador conserva asignación manual y retiro. Un retiro deja ese servicio bajo control manual, para impedir que el proceso automático revierta la decisión. La disponibilidad utilizada es la registrada en el sistema y no una garantía de presencia en línea. La creación de Motorista 2 fue confirmada en la salida de terminal aportada; no se incluyen credenciales en este informe.

### Formularios y experiencia de uso

Se inicializó un campo obligatorio omitido en el alta de usuarios; se añadieron validaciones y mensajes junto a los formularios. Se corrigieron contrastes, distribución de descripciones y fechas en móvil. Las tarjetas del motorista se hicieron navegables y se unificó el título de cada sección. El documento HTML pasó de inglés a español y se desaconsejó la traducción automática; se trata de una corrección relacionada con los textos alterados, no de una prueba concluyente de la causa en el navegador de los compañeros.

## 7 Evidencias y validación

| Evidencia | Resultado disponible | Límite de la conclusión |
| --- | --- | --- |
| `npm run check` | Aprobado en la revisión del 30 de septiembre | Verifica TypeScript y plantillas, no la experiencia visual completa |
| `npm run test:logic` | 17 casos aprobados en la última revisión registrada | Pruebas en proceso con sustitutos; no integración completa con SQL ni GPS real |
| `dotnet build --no-restore` | Compilación aprobada, sin errores ni advertencias en la última revisión | No confirma reglas de negocio contra datos reales |
| Pruebas de seguridad y reglas | 22 comprobaciones aprobadas en la revisión de asignaciones | Ejecución anterior al último ajuste de formularios; no todas se repitieron el día 30 |
| Salida Angular del usuario | Generación de paquete de desarrollo correcta y servidor en modo observación | No acredita compilación optimizada de producción |
| Confirmación del usuario | Carrito y totales correctos; ubicación funcionó en celular | No implica aprobación de todos los módulos |
| Salida de creación de cuenta | Usuario y perfil de Motorista 2 insertados | No acredita todavía asignación automática simultánea |
| Salida Git aportada | Push `a999e4c..4e188e8 main -> main` completado | La subida del código no publica un servidor permanente ni la base de datos |

Los resultados son los registrados durante el desarrollo; no se ejecutaron nuevamente suites de software para redactar este documento. La última revisión reúne 17 casos frontend y existe evidencia anterior de 22 comprobaciones backend; no se presenta su suma como una única prueba integral.

## 8 Resultados y limitaciones

Se dispone de una versión consolidada con mejoras de carrito, sesiones, ubicación, flujo guiado y administración. El proyecto conserva las imágenes de referencia del menú. Se ha reducido la ambigüedad de mensajes y el riesgo de operar sobre copias antiguas. El historial de desarrollo queda trazado con incidencias numeradas y un checkpoint asociado a un commit real.

Permanecen pendientes: aceptación visual de los últimos cambios en móvil; alta de usuarios después de la corrección; pruebas de asignaciones concurrentes y retiro manual; instalación en otra computadora; paquete de producción; correo SMTP autorizado; restauración de respaldo de base de datos; y evaluación de desempeño con volumen representativo. El GPS requiere permisos y no garantiza seguimiento con aplicación cerrada o pantalla bloqueada. El mapa muestra puntos, no cálculo de ruta ni tiempo estimado.

## 9 Relación con la propuesta de empresa ficticia

Se elaboró por separado un presupuesto académico con inversión de Q50,000 y equilibrio de 670 servicios mensuales para una mezcla de 80% comida y 20% paquetes. Estos resultados dependen de supuestos, incluida una comisión propuesta al restaurante. No constituyen ingresos reales ni prueban que las comisiones, liquidaciones y reportes financieros estén implementados. El documento económico puede anexarse al informe, manteniendo esa separación.

## 10 Conclusiones preliminares

Las pruebas han mostrado que un componente que compila puede seguir fallando por datos incompletos, permisos del dispositivo o presentación en pantalla. La respuesta consistió en corregir causas concretas, conservar reglas de autorización y añadir comprobaciones antes de dar por cerrado cada incidente. La principal conclusión del checkpoint es que existe una base funcional consolidada, pero todavía se requiere una prueba integral de aceptación para sostener que el ciclo completo está terminado.

El siguiente checkpoint debe incluir servicios de prueba identificados sin datos personales, resultados por rol y evidencias sanitizadas. El informe DERCAS final podrá construirse a partir de este registro sin depender únicamente de reconstruir conversaciones.

## 11 Fuentes y anexos

- Conversación previa “Diseñar plataforma delivery”, recuperada parcialmente; disponible el resumen de idea, roles, arquitectura y mapas. El usuario aclaró posteriormente que DERCAS se refiere al manual o tutorial del proceso y sus problemas.
- Conversación actual: solicitudes, capturas, confirmaciones y salidas de terminal del usuario.
- Git: `a999e4c` del 17 de septiembre y `4e188e8` del 30 de septiembre de 2026.
- Documentación del repositorio: `ALCANCE_Y_ESTADO.md`, `MOTORISTAS_Y_ASIGNACION.md`, `REVISION_CAPTURAS_30_SEPTIEMBRE.md` y `PRUEBAS.md`.
- [Tutorial de implementación y uso](MANUAL_PRACTICO.md), [bitácora de incidencias](BITACORA.md) y [checkpoint y plan de aceptación](CHECKPOINTS.md).

No se adjuntan contraseñas, tokens, configuraciones privadas, coordenadas reales ni datos personales de clientes. Las capturas originales quedan en la conversación; para una entrega académica deben prepararse copias anonimizadas.
