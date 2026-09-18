const catalogo = [
  { nombre: 'Desayuno Chapin', descripcion: 'Huevos, frijoles, platano y tortillas', archivo: 'desayuno-chapin' },
  { nombre: 'Pollo a la plancha', descripcion: 'Pollo a la plancha acompañado de papas y ensalada', archivo: 'pollo-plancha' },
  { nombre: 'Hamburguesa Regional', descripcion: 'Hamburguesa de carne, queso, vegetales y papas', archivo: 'hamburguesa-regional' },
  { nombre: 'Gaseosa', descripcion: 'Bebida gaseosa personal', archivo: 'gaseosa' }
];
const normalizar = (valor: unknown): string => String(valor ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().replace(/[^a-z0-9]+/g, ' ').trim();
export function presentarProducto(producto: any): any {
  const referencia = catalogo.find(item => normalizar(item.nombre) === normalizar(producto.nombre) && normalizar(item.descripcion) === normalizar(producto.descripcion));
  return { ...producto, imagen: producto.imagen?.trim() || (referencia ? '/images/menu/' + referencia.archivo + '.png' : null), imagenReferencial: !producto.imagen?.trim() && !!referencia };
}
