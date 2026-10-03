# Checkpoints y continuidad del proyecto

## Historial verificable

| Registro | Fecha verificable | Referencia | Alcance |
| --- | --- | --- | --- |
| Base previa | 17 de septiembre de 2026 | a999e4c | Proyecto Regional Express, avance general |
| CP-001 | 30 de septiembre de 2026 | 4e188e8 | Consolidación: 58 archivos, 1,971 inserciones y 1,814 eliminaciones |

Las cantidades de líneas no representan horas trabajadas, porcentaje de avance ni calidad. No hay registro fiable de horas para calcularlas retrospectivamente.

## CP-001

- Commit completo: `4e188e845ecd11ea9d39abf1d83ad08524e57ec4`.
- Rama: `main`.
- Repositorio: https://github.com/javiercastro808/RegionalExpress.
- Fecha de autor registrada en Git: 2026-09-30T19:40:19-06:00.
- Fuente de confirmación del push: salida de terminal aportada por el usuario, con actualización `a999e4c..4e188e8 main -> main`.
- Carpeta operativa: `C:\Users\polo-\Documents\My Web Sites\RegionalExpress`.
- Situación al iniciar este registro: sin cambios en archivos versionados; existe un archivo ajeno sin seguimiento con nombre anómalo. No se eliminó ni incorporó.
- Estos documentos se crean después del commit citado. Son documentación local nueva; no se afirma que ya estén en GitHub. No se creó tag, commit, push ni respaldo de SQL para este checkpoint documental.
- Arranque local: `scripts/Iniciar-Pruebas.ps1`; página en puerto 4300 y API interna en 5292.
- Pruebas con compañeros: enlace HTTPS temporal de VS Code; el equipo anfitrión y la base deben permanecer disponibles.
- Última validación frontend registrada: 17 casos de lógica y revisión de plantillas aprobados.
- Backend: compilación aprobada; 22 comprobaciones de seguridad, coordenadas y elegibilidad documentadas en la revisión previa de asignaciones.
- Confirmaciones de usuario: carrito correcto, GPS disponible desde celular y creación de Motorista 2.
- Pendientes principales: aceptación visual, altas contra SQL y operación simultánea con dos motoristas.

## Próximo checkpoint CP-002 propuesto

Objetivo: validar el ciclo operativo completo y cerrar incidencias con evidencia real.

| Prueba | Resultado esperado | Evidencia que debe guardarse |
| --- | --- | --- |
| Dos cuentas CLIENTE/MOTORISTA creadas desde admin | Alta o validación clara de campos; sin error genérico | Captura sin credenciales y respuesta funcional |
| Pedido con varios productos | Cantidades y total correctos; Q10 domicilio y Q0 recoger | Totales antes/después, identificador de servicio anonimizado |
| Dos servicios elegibles y dos motoristas libres | Cada servicio asignado sin duplicar una ocupación | Asignación e historial por servicio |
| Todos los motoristas ocupados | Servicio espera; se asigna cuando uno queda elegible | Estado pendiente y asignación posterior |
| Retiro administrativo | Servicio permanece bajo control manual hasta asignación del admin | Historial de retiro y nueva asignación |
| Confirmación de pasos | Solo siguiente estado; no retrocesos ni saltos | Secuencia de estados y mensajes de conflicto |
| Origen, cliente y motorista en sitios distintos | Tres puntos identificados; motorista visible al llegar actualización | Captura con ubicación anonimizada |
| Precisión GPS y pérdida de señal | Se explica aproximación; no se inventa posición ni se avanza sin confirmación reciente | Mensajes sin coordenadas personales |
| Mapa e historial en móvil | Ampliar/reducir funciona; fechas y títulos no se cortan | Capturas por ancho de pantalla |
| Finalización | Historial registrado y disponibilidad coherente del motorista | Estado final y disponibilidad |
| Acceso por rol | Un usuario ajeno no consulta datos privados de otro servicio | Matriz de accesos esperados/obtenidos |
| Compilación de producción e instalación | Build optimizado e inicio en entorno nuevo | Salida de comandos y requisitos usados |

## Protocolo de actualización

1. Antes de cambios relevantes, registrar versión de código, problema, evidencia inicial y alcance previsto.
2. Al corregir, actualizar BITACORA.md con causa, cambio y prueba. Conservar la distinción entre implementado y validado.
3. Al cerrar una etapa, añadir un checkpoint con fecha, commit real, resultados, pendientes y responsable. Si aún no hay commit, indicar “sin versionar”.
4. Guardar solo evidencias anonimizadas. No incluir contraseñas, tokens, archivos de configuración, datos reales de clientes ni coordenadas precisas.
5. Actualizar el informe con resultados comprobados, no con promesas de funcionalidades.
6. Subir la documentación junto con el siguiente commit autorizado. Los registros no se actualizan solos fuera de las sesiones de trabajo.

## Plantilla de checkpoint

- ID y fecha:
- Commit y rama, o sin versionar:
- Objetivo de la etapa:
- Incidencias incluidas:
- Cambios y decisiones:
- Pruebas ejecutadas y resultados:
- Evidencias y quién validó:
- Limitaciones conocidas:
- Pendientes y siguiente acción:
- Respaldo de base de datos: realizado / no realizado; referencia privada, nunca credenciales:
- Estado de publicación: local / subido a Git / desplegado y comprobado:

## Documentos relacionados

- [Informe inicial](INFORME_INICIAL.md).
- [Bitácora](BITACORA.md).
- Los documentos anteriores del repositorio se conservan como cortes históricos. El informe del 24 de septiembre contiene pendientes que se resolvieron después, como la subida a GitHub y la creación de la segunda cuenta; consultar este checkpoint para el estado consolidado.
