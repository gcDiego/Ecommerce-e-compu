# Funcionalidad pendiente de migrar

El proyecto `Front.Web` ya consume los microservicios de catálogo, identidad y carrito. `CapaPresentacionTienda` permanece como aplicación legacy y referencia funcional durante la transición.

## Microservicios pendientes

### Customer / Accounts

- Registro de clientes.
- Recuperación de contraseña por correo.
- Persistencia del cambio obligatorio de contraseña.
- Perfil y actualización de datos del cliente.

### Location

- Consulta de estados.
- Consulta de municipios por estado.
- Consulta de localidades por municipio.
- Definir si se utilizará un catálogo propio o un proveedor externo.

### Orders / Sales

- Creación de órdenes desde el carrito.
- Persistencia del detalle de venta.
- Historial de compras del cliente.
- Consulta de transacciones.
- Descuento transaccional de inventario.
- Vaciar el carrito después de confirmar una orden.

### Payments

- Creación de órdenes de PayPal.
- Captura y confirmación del pago.
- URLs de retorno y cancelación configurables.
- Idempotencia y conciliación de pagos.
- Manejo de webhooks de PayPal.

### Catalog Administration

- Alta, edición y desactivación de productos.
- Administración de categorías y marcas.
- Carga y almacenamiento de imágenes.
- Control de stock desde una API protegida para administradores.

### Media / Product Images

- El contrato actual de `Catalog.Api` no incluye una URL de imagen.
- Definir almacenamiento de imágenes y agregar `imageUrl` al producto.
- Mientras se migra, `Front.Web` utiliza imágenes ilustrativas de los prototipos.

## Funcionalidad temporal del Front

- El panel de envío se presenta deshabilitado hasta disponer de Location, Orders y Payments.
- Registro y recuperación no se muestran como acciones disponibles hasta disponer de Customer/Accounts.
- `admin.html` permanece como prototipo; no debe conectarse directamente a la base de datos legacy.

## Criterios para retirar el proyecto legacy

- Todas las operaciones anteriores están disponibles mediante APIs.
- Los flujos de compra y pago tienen pruebas de integración.
- Los JWT y secretos se configuran fuera del repositorio.
- Los datos históricos de ventas pueden consultarse desde el nuevo Front.
- No quedan llamadas desde `Front.Web` hacia `CapaNegocio` ni hacia la base de datos legacy.
