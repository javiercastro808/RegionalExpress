# Regional Express — alcance y estado

Actualizado: 24 de septiembre de 2026.
Carpeta de trabajo: C:\Users\polo-\Documents\My Web Sites\RegionalExpress.
Repositorio: https://github.com/javiercastro808/RegionalExpress.git

## Implementado o corregido

- Carrito con varios productos, cantidades, importes por ítem y subtotal general. Cálculos en centavos; envío Q10 a domicilio y Q0 al recoger, sin acumulación al alternar. Impide mezclar restaurantes.
- Restaurante: catálogo filtrado por restaurante y categoría, carga al cambiar de ruta, productos disponibles e imágenes de comida conservadas según nombre y descripción. Carga diferida de imágenes.
- Sesiones separadas por pestaña, con carrito por usuario y pestaña; cerrar una sesión no debe cerrar las demás. Recargar conserva la sesión de esa pestaña.
- Pedido a domicilio: ubicación requerida. El cliente puede usar GPS o seleccionar un punto del mapa; recoger no requiere destino de entrega.
- Envíos: origen y destino obligatorios en mapa y validados también por la API.
- Motorista: ubicación reciente y confirmada necesaria para avanzar los estados correspondientes; actualización GPS mientras mantiene abierta la página. Consulta automática de servicios cada 20 segundos y aviso interno de nuevas asignaciones.
- Asignación: el panel usa el identificador real del motorista para mostrar si está asignado. Se excluyen recogidas y servicios finalizados de los pendientes asignables.
- Rastreo autorizado con origen, última posición del motorista y destino. Son puntos geográficos, no una ruta calculada por calles. No expone ubicación privada a usuarios sin acceso al servicio.
- Historial del restaurante conectado a la API, con estado, fecha y actor; corregida actualización de sus vistas y del panel administrativo tras respuestas de la API.
- Se mantienen páginas y rutas de inicio, delivery, restaurante, envíos, rastreo y paneles por rol. No se agregó provideZoneChangeDetection.
- Se mantienen autenticación, contraseñas protegidas, autorización por rol, comprobación de precios en servidor y códigos de rastreo.
- Corregida referencia del script de GitHub al repositorio RegionalExpress. No se ejecutó commit ni push.

## Verificación realizada

- Angular: npm run check aprobado (TypeScript y plantillas).
- Lógica frontend: 11 pruebas aprobadas, incluyendo 100 alternancias de modalidad, separación de sesiones/carritos, restricciones geográficas, asignaciones e imágenes.
- Backend: dotnet build --no-restore aprobado, sin errores ni advertencias.
- Seguridad y coordenadas: 14 comprobaciones aprobadas.
- Estas comprobaciones no sustituyen una prueba completa con SQL Server, navegador y teléfonos reales.
- La generación completa del paquete Angular había quedado bloqueada por permisos del proceso de compilación en este entorno; sigue pendiente de confirmar en una terminal local.

## Pendientes para dar por cerrado el funcionamiento

1. Ejecutar npm run build localmente y probar el flujo completo con base de datos: cliente crea → administrador asigna → motorista recibe y actualiza → cliente rastrea → entrega e historial.
2. Probar en paralelo los cuatro roles en pestañas distintas y comprobar acceso permitido y denegado con cuentas reales de prueba.
3. Verificar mapas y GPS en teléfonos mediante HTTPS, permisos denegados, pérdida de señal y reconexión. El navegador no garantiza seguimiento con pantalla bloqueada o aplicación cerrada.
4. Revisar visualmente imágenes, categorías, cantidades, totales y adaptación móvil; verificar que los textos visibles dicen Entrega a domicilio.
5. Probar instalación desde cero en otra PC con el esquema SQL, configuración privada y datos de demostración. Los datos de producción no están incluidos.
6. Configurar y comprobar correo SMTP con destinatarios de prueba autorizados. Existe integración; no se enviaron correos reales durante esta revisión.
7. Revisar los cambios locales antes de guardar la versión en GitHub; existe contenido previamente preparado por el usuario. No reaplicar el ZIP antiguo porque perdería correcciones posteriores.

## Mejoras posteriores

- Paginación del historial y listados extensos; medir tiempos con datos reales antes de ampliar optimizaciones.
- Reportes administrativos y avisos adicionales según necesidades operativas.
- Ruta por calles y tiempo estimado mediante un proveedor de mapas, si se requiere; todavía no implementados.
- Seguimiento en segundo plano fiable requiere evaluar una solución móvil adecuada.
- Ampliar fotografías específicas del catálogo manteniendo correspondencia con cada plato y su descripción.

Estado: avance funcional implementado y comprobado a nivel de código; pendiente de aceptación integral en el entorno real. No se declara terminado al 100%.
