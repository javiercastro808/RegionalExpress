# Pruebas funcionales antes de compartir

Usar datos ficticios y cuentas de demostración. Ejecutar `dotnet run --project tests/Seguridad` desde la raíz para comprobar hashes y migración. `npm run check` desde frontend comprueba TypeScript y plantillas.

1. Acceder como cliente: Inicio y Mi panel llevan a su panel. El historial muestra solamente sus servicios. Al entrar con otro cliente no debe aparecer ese historial.
2. Crear pedido DOMICILIO: dirección obligatoria, precios tomados del catálogo y delivery Q10. Un producto de otro restaurante o no disponible debe rechazarse; cantidades fuera de 1–100 también.
3. Crear pedido RECOGER: delivery Q0; no debe ofrecer asignación ni mapa de motorista.
4. Crear envío: debe pertenecer al cliente conectado. No se admite crear pedido/envío sin sesión CLIENTE. El doble clic no duplica una solicitud en curso.
5. Como administrador, asignar y retirar un motorista: comprobar notificación, nombre y disponibilidad tras cada acción. Los finalizados no admiten asignación.
6. Como motorista, compartir ubicación con permiso del navegador. El cliente propietario y el administrador deben verla; otra cuenta y un visitante anónimo no reciben coordenadas. Cerrar la página o dejar de compartir detiene los envíos.
7. Actualizar estado hasta finalizar. Confirmar historial, disponibilidad y ausencia de mapa activo al finalizar.
8. Desactivar un usuario desde administración: su token previo debe dejar de funcionar. Una cuenta nueva o contraseña cambiada se guarda como hash. Las cuentas heredadas migran al entrar en Development, o mediante el comando de migración explícito.
9. En alojamiento HTTPS, abrir enlaces directos como `/panel/cliente` y `/rastreo?codigo=...` y refrescar. Deben funcionar sin localhost. `/api/no-existe` debe responder 404, no devolver el HTML de Angular.
10. Apagar el PC del desarrollador y probar desde otro dispositivo. Esta es la comprobación final del alojamiento independiente.

El GPS depende del permiso y del dispositivo real. La tarifa de envío usa peso y distancia declarados: no calcula aún una ruta real por calles. El correo real requiere un proveedor SMTP configurado.
