import { inject } from "@angular/core";
import { Auth } from "./auth";
import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

export interface ItemCarrito {
  idProducto: number;
  idRestaurante?: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  cantidad: number;
  imagen?: string | null;
}

@Injectable({
  providedIn: "root",
})
export class Carrito {
  private auth = inject(Auth);
  private usuarioId = this.auth.getUsuario()?.idUsuario ?? 0;
  private get storageKey() {
    return "regionalExpressCarrito.v3." + this.usuarioId;
  }

  private items: ItemCarrito[] = [];

  private itemsSubject = new BehaviorSubject<ItemCarrito[]>([]);

  items$ = this.itemsSubject.asObservable();

  constructor() {
    this.cargarCarrito();
    this.auth.usuario$.subscribe((usuario) => {
      const id = usuario?.idUsuario ?? 0;
      if (id !== this.usuarioId) {
        this.usuarioId = id;
        this.items = [];
        this.cargarCarrito();
      }
    });
  }

  private cargarCarrito(): void {
    let guardado: string | null = null;
    try {
      guardado = sessionStorage.getItem(this.storageKey);
    } catch {}

    if (guardado) {
      try {
        const datos = JSON.parse(guardado);
        this.items = Array.isArray(datos)
          ? datos.filter(
              (i) =>
                Number.isSafeInteger(i.idProducto) &&
                typeof i.nombre === "string" &&
                Number.isFinite(i.precio) &&
                i.precio >= 0 &&
                Number.isSafeInteger(i.cantidad) &&
                i.cantidad > 0 &&
                i.cantidad <= 99 && Number.isSafeInteger(i.idRestaurante) && i.idRestaurante>0,
            )
          : [];
      } catch {
        this.items = [];
      }
    }

    this.emitirCambios();
  }

  agregar(producto: any): boolean {
    const restaurante = Number(producto.idRestaurante);
    if(!Number.isSafeInteger(restaurante)||restaurante<=0)return false;
    if(this.items.length && this.items.some(i=>i.idRestaurante !== restaurante)) return false;
    if(!Number.isSafeInteger(Number(producto.idProducto))||!Number.isFinite(Number(producto.precio))||Number(producto.precio)<0)return false;
    const id = Number(producto.idProducto);

    const existente = this.items.find((item) => item.idProducto === id);

    if (existente) {
      existente.cantidad = Math.min(99, existente.cantidad + 1);
    } else {
      this.items.push({
        idProducto: id,
        idRestaurante: Number.isSafeInteger(restaurante)&&restaurante>0 ? restaurante : undefined,

        nombre: producto.nombre,

        descripcion: producto.descripcion,

        precio: Number(producto.precio),

        cantidad: 1,

        imagen: producto.imagen,
      });
    }

    this.emitirCambios();
    return true;
  }

  aumentar(idProducto: number): void {
    const item = this.items.find((x) => x.idProducto === idProducto);

    if (!item) {
      return;
    }

    item.cantidad = Math.min(99, item.cantidad + 1);

    this.emitirCambios();
  }

  disminuir(idProducto: number): void {
    const item = this.items.find((x) => x.idProducto === idProducto);

    if (!item) {
      return;
    }

    if (item.cantidad > 1) {
      item.cantidad -= 1;
    } else {
      this.eliminar(idProducto);

      return;
    }

    this.emitirCambios();
  }

  eliminar(idProducto: number): void {
    this.items = this.items.filter((x) => x.idProducto !== idProducto);

    this.emitirCambios();
  }

  obtenerItems(): ItemCarrito[] {
    return this.items.map(i=>({...i}));
  }

  obtenerCantidadProducto(idProducto: number): number {
    return this.items.find((x) => x.idProducto === idProducto)?.cantidad ?? 0;
  }

  obtenerCantidadTotal(): number {
    return this.items.reduce((total, item) => total + item.cantidad, 0);
  }

  obtenerSubtotalItem(item: ItemCarrito): number {
    return Math.round(item.precio*100)*item.cantidad/100;
  }

  obtenerSubtotal(): number {
    return this.items.reduce(
      (total, item) => total + Math.round(item.precio*100) * item.cantidad,
      0,
    ) / 100;
  }

  limpiar(): void {
    this.items = [];

    this.emitirCambios();
  }

  private emitirCambios(): void {
    try {
      sessionStorage.setItem(this.storageKey, JSON.stringify(this.items));
    } catch {}

    this.itemsSubject.next([...this.items]);
  }
}
