# Revisión del 30 de septiembre

- Admin: etiquetas con contraste explícito, controles legibles y descripciones separadas de direcciones.
- Alta de usuarios: se inicializa Apellido (campo obligatorio de SQL) y FechaRegistro. Contraseñas nuevas de al menos 8 caracteres; validación de correo y longitudes. Los mensajes de validación se muestran junto al formulario.
- Crear un motorista: primero crear el usuario con rol MOTORISTA y después su perfil en Motoristas. Se conserva este flujo administrativo.
- Las cuentas de gestión no muestran enlaces para pedir comida o crear envíos en la navegación; en el restaurante se indica que se necesita cuenta CLIENTE y se desactivan los botones de compra.
- Motorista: tarjetas pulsables, filtro de activos, título único por sección e historial adaptable a móvil.
- Documento HTML en español y traducción automática desactivada para evitar alteraciones como Entrega errónea.
- GPS: continúa siendo obligatorio, reciente y confirmado por servidor. Se aceptan posiciones con precisión de hasta 500 m. Entre 150 y 500 m se advierte que es aproximada; se conserva el reintento. No se añaden coordenadas ficticias. El mapa indica que GPS es aproximado.
- Rastreo: mapa ampliado al consultar, ocupa 75% del alto visible, con botón para reducir/ampliar y reajuste de puntos. Selección de ubicación: botón de ampliación hasta 70% del alto visible.

Validación: compilador Angular, pruebas de lógica y compilación .NET. Pendiente revisar visualmente en teléfono y comprobar altas en SQL: localhost:4300 no estaba ejecutándose durante la revisión. Reiniciar con scripts/Iniciar-Pruebas.ps1 para cargar los cambios de API.
