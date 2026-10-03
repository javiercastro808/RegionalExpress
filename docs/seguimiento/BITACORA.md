# Bitácora de problemas y soluciones

Corte: 30 de septiembre de 2026 · Referencia: CP-001 / `4e188e8`

Registro retrospectivo. Los identificadores representan orden de reconstrucción, no fechas exactas ni commits independientes. “Confirmado” significa que existe evidencia expresada; “corregido en código” conserva una validación pendiente. No se cierra un incidente solo porque desapareció el error de compilación.

| ID | Problema observado | Causa o explicación | Cambio realizado | Evidencia y estado | Validación pendiente |
| --- | --- | --- | --- | --- | --- |
| INC-001 | HTML dejó de compilar al reemplazar restaurante.ts | Nombres de métodos incompatibles con la plantilla, incluido subtotal | Conservación de métodos esperados y revisión Angular | Corregido; compilación y uso posterior del carrito | Mantener prueba de regresión |
| INC-002 | Envío se duplicaba al alternar domicilio/recoger | Lógica de total necesitaba derivarse de la modalidad sin acumular cargos | Q10/Q0, cantidades, subtotales y cálculos en centavos | Confirmado por usuario; prueba de 100 alternancias | Ninguna específica del caso reproducido |
| INC-003 | Una sesión interfería con otra durante las pruebas | Estado de autenticación compartido entre pestañas | Sesión por pestaña y carrito separado por cuenta | Corregido; prueba automatizada | Aceptación con cuatro roles en paralelo |
| INC-004 | Imágenes de comida perdidas al reconstruir | Reemplazo de archivos desde una copia anterior | Recuperación de recursos y correspondencia con descripción | Corregido; pruebas de referencias de imágenes | Revisión de todo el catálogo |
| INC-005 | Dos copias locales y ZIP podían sobrescribir avances | Orígenes de trabajo diferentes | Definición de carpeta canónica y advertencia de no reaplicar ZIP viejo | Carpeta verificada; consolidación Git | Evitar nuevas divergencias |
| INC-006 | Servicio asignado figuraba como no asignado | Inferencia visual a partir de pendientes y datos almacenados en memoria | Uso del identificador real del motorista | Corregido; prueba de lógica | Reasignación entre dos cuentas reales |
| INC-007 | Faltaban puntos geográficos de los envíos | Origen/destino no se persistían completos | Formularios y validación de coordenadas en API | Corregido; pruebas de obligatoriedad y transmisión | Creación y rastreo contra SQL |
| INC-008 | Historial del restaurante vacío o vistas sin actualizar | Historial desconectado y actualización visual incompleta | Endpoint de historial y notificación de cambios de vista | Corregido en código; compilación | Aceptación del encargado del restaurante |
| INC-009 | Inicio fallaba con Missing script start:pruebas | Comando ausente en package.json | Comando de inicio con configuración y proxy correctos | Confirmado por salida de arranque del usuario | Ninguna específica del arranque registrado |
| INC-010 | Motorista no podía pulsar cambio de estado | Botón bloqueado por ubicación y error fuera del modal | Mensaje dentro del detalle y explicación del requisito | Corregido; usuario continuó diagnóstico GPS | Aceptación del flujo completo |
| INC-011 | Permiso GPS concedido pero espera sin resultado | Permiso no garantiza respuesta del dispositivo | Tiempo máximo, reintento y descarte de callback tardío | Prueba automatizada; usuario confirmó GPS en celular | GPS en distintas condiciones/dispositivos |
| INC-012 | Rastreo parecía mostrar solo al cliente | Se encontró encuadre inicial que no incluía motorista recibido después | Reencuadre al aparecer motorista, etiquetas y ver todos | Corregido; prueba de puntos separados | Confirmación visual con dos ubicaciones reales |
| INC-013 | Selección manual de estados poco clara | Interfaz mostraba estados en vez de próxima acción | Confirmar siguiente paso; API rechaza saltos | Corregido; prueba de secuencia | Entrega completa y cambios simultáneos |
| INC-014 | Se necesitaba reparto automático y segundo motorista | Funcionalidad solicitada, no fallo previo | Asignador, control manual tras retiro y creador de cuenta | Cuenta confirmada en SQL por salida del usuario; reglas probadas | Simultaneidad y distribución real |
| INC-015 | Alta de usuarios fallaba con mensaje genérico | Se encontró Apellido obligatorio sin inicializar | Inicializar campo y fecha, validar correo/clave y mostrar errores | Corrección de código compilada | Crear CLIENTE y MOTORISTA desde panel contra SQL |
| INC-016 | Rol/estado poco legibles y descripción invadía dirección | Contraste y estilos de contenido insuficientes | Colores explícitos, ajuste de texto y separación visual | Estilos aplicados | Captura de aceptación en PC y móvil |
| INC-017 | Títulos unidos o traducidos como Entrega errónea | HTML declarado en inglés; traducción automática como posible factor | Español, no traducción y un solo título calculado | Corregido; prueba de títulos y filtros | Revisión en navegador de compañeros |
| INC-018 | Tarjetas parecían botones sin navegar y fecha salía del historial | Tarjetas sin acción y distribución rígida | Botones de sección, filtro de activos y ajuste de fecha | Pruebas de navegación lógica | Validación visual móvil |
| INC-019 | GPS de 245 m impedía continuar | Regla anterior exigía precisión de 150 m | Límite 500 m, aviso de aproximación y confirmación del servidor | Prueba: acepta 245 y rechaza 501 m | Prueba real y evaluación operativa del margen |
| INC-020 | Mapa pequeño al consultar | Alto fijo insuficiente | Rastreo ampliado a 75% del alto visible; selección ampliable a 70% | Código compilado | Pantallas pequeñas y redimensionamiento |
| INC-021 | Cuenta administradora intentaba comprar | Diferencia entre acceso de gestión y rol CLIENTE | Navegación por rol y explicación en carrito | Conserva autorización; código revisado | Revisión de navegación por cada rol |
| INC-022 | Dudas para subir al mismo repositorio | Copias y referencia anterior del script | Corrección del remoto y uso del script de publicación de código | Push confirmado a main, commit 4e188e8 | Nuevos documentos aún sin commit |

## Lecciones aprendidas

1. Corregir componentes conservando el contrato entre plantilla y clase; evitar reemplazos completos sin comparar versiones.
2. Separar el precio de los productos del cargo de envío; calcular el total desde el estado actual.
3. Distinguir permiso GPS, posición recibida, precisión y confirmación de servidor.
4. Diferenciar reglas de acceso de problemas visuales; no ampliar permisos para ocultar un error de interfaz.
5. Registrar evidencia de usuario además de pruebas automatizadas; compilación y aceptación no son equivalentes.
6. Consolidar la carpeta de trabajo antes de compartir ZIP o subir código.

## Plantilla para nuevas incidencias

- ID: INC-023
- Fecha y responsable del reporte:
- Rol, dispositivo y versión de código:
- Pasos para reproducir:
- Resultado esperado y resultado observado:
- Causa comprobada o hipótesis:
- Cambio aplicado y archivos afectados:
- Prueba y evidencia sanitizada:
- Estado: abierto / en análisis / corregido en código / validado por usuario / cerrado
- Pendiente para cerrar:
- Commit y checkpoint relacionados:

Los registros existentes se actualizan añadiendo evidencia de cierre; no se borra el síntoma original ni se transforma una hipótesis en causa comprobada sin validación.

## INC-023 e INC-024 — 3 de octubre de 2026

INC-023: cartografía bloqueada por proveedor (403). Se corrige Referer y se informa del fallo; pendiente confirmar desbloqueo en navegador del usuario.

INC-024: ampliación limitada a la columna. Se implementa diálogo amplio, cierre con botón/Escape y restauración del mismo mapa. Pruebas DOM aprobadas; aceptación visual pendiente.

Referencia: docs/REVISION_MAPAS_03_OCTUBRE.md. Cambios locales posteriores a CP-001, todavía sin commit.
