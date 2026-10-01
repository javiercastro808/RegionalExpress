# Motoristas y asignaciones

El motorista confirma el siguiente paso desde el detalle del servicio. La API rechaza saltos, retrocesos y servicios finalizados; la ubicación reciente sigue siendo obligatoria. La cancelación queda fuera de la secuencia normal del motorista.

Cada 15 segundos el servidor busca servicios sin asignar. Delivery a domicilio espera un estado que contenga Listo o Motorista; recoger se excluye. Los envíos pendientes pueden asignarse desde su solicitud. Se elige un motorista activo, con usuario activo, disponible y sin otro servicio activo, priorizando quien tiene menos servicios históricos. Si todos están ocupados, espera al siguiente ciclo. No representa disponibilidad GPS ni presencia en línea: administra el indicador Disponible desde el panel.

El administrador puede retirar y luego asignar otro motorista. Retirar deja el servicio bajo control manual, sin reasignación automática. El historial registra las asignaciones y retiros. La asignación automática se identifica como Sistema (cuenta interna inactiva, sin inicio de sesión).

Para crear Motorista 2, detener las pruebas y ejecutar scripts/Crear-Motorista-Prueba.ps1. La cuenta motorista2@regionalexpress.test se crea con contraseña aleatoria que aparece en la terminal. No reemplaza cuentas existentes. Después reiniciar scripts/Iniciar-Pruebas.ps1.

Verificación: compilación de API y Angular, pruebas de secuencia y elegibilidad. Pendiente: prueba integral contra SQL con dos motoristas, pedidos simultáneos, liberación al entregar y retiro manual. La creación de la cuenta desde el entorno del asistente no pudo conectar con LocalDB; debe ejecutarse desde la terminal local.
