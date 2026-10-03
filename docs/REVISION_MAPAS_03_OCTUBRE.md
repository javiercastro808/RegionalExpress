# Mapas y ventana ampliada — 3 de octubre de 2026

Problema: capturas con Access blocked 403 de OpenStreetMap; ampliar solo cambiaba el alto dentro de la columna.

Implementación: componente compartido VentanaMapa con diálogo nativo de 98% del ancho y 96% del alto visible, cierre por botón o Escape. Se mueve y restaura el mismo contenedor Leaflet, conservando marcador, coordenadas y eventos. Capa común con referrerPolicy strict-origin-when-cross-origin, atribución visible, caché normal y mensajes de error. No se usan proxies, identidades falsas ni evasión del bloqueo.

La causa específica del 403 no se puede demostrar solo con la captura. La corrección de Referer cumple la configuración esperada, pero no garantiza retirar bloqueos de IP, navegador integrado o proveedor. Validar en el navegador HTTPS real del usuario; si persiste, configurar un proveedor autorizado con su propia clave. CARTO actualmente exige clave; no se añadió un endpoint sin autorización.

Política consultada: https://operations.osmfoundation.org/policies/tiles/

Pruebas: compilación de plantillas Angular; prueba de configuración y mensajes de cartografía; prueba DOM de ampliar y restaurar el mismo nodo sin pérdida de tamaño ni posición original. Pendiente prueba visual y de teselas en la red afectada. No se cambió la base de datos.
