import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ItemCarrito {
  idProducto: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  cantidad: number;
  imagen?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class Carrito {

  private readonly storageKey = 'regionalExpressCarrito';

  private items: ItemCarrito[] = [];

  private itemsSubject =
    new BehaviorSubject<ItemCarrito[]>([]);

  items$ =
    this.itemsSubject.asObservable();

  constructor() {

    this.cargarCarrito();

  }

  private cargarCarrito(): void {

    const guardado =
      localStorage.getItem(this.storageKey);

    if (guardado) {

      try {

        this.items =
          JSON.parse(guardado);

      } catch {

        this.items = [];

      }

    }

    this.emitirCambios();

  }


  agregar(producto: any): void {

    const id =
      Number(producto.idProducto);

    const existente =
      this.items.find(
        item =>
          item.idProducto === id
      );

    if (existente) {

      existente.cantidad += 1;

    } else {

      this.items.push({

        idProducto: id,

        nombre:
          producto.nombre,

        descripcion:
          producto.descripcion,

        precio:
          Number(producto.precio),

        cantidad: 1,

        imagen:
          producto.imagen

      });

    }

    this.emitirCambios();

  }


  aumentar(idProducto: number): void {

    const item =
      this.items.find(
        x =>
          x.idProducto === idProducto
      );

    if (!item) {
      return;
    }

    item.cantidad += 1;

    this.emitirCambios();

  }


  disminuir(idProducto: number): void {

    const item =
      this.items.find(
        x =>
          x.idProducto === idProducto
      );

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

    this.items =
      this.items.filter(
        x =>
          x.idProducto !== idProducto
      );

    this.emitirCambios();

  }


  obtenerItems(): ItemCarrito[] {

    return [...this.items];

  }


  obtenerCantidadProducto(
    idProducto: number
  ): number {

    return (
      this.items.find(
        x =>
          x.idProducto === idProducto
      )?.cantidad ?? 0
    );

  }


  obtenerCantidadTotal(): number {

    return this.items.reduce(
      (total, item) =>
        total + item.cantidad,
      0
    );

  }


  obtenerSubtotalItem(
    item: ItemCarrito
  ): number {

    return (
      item.precio *
      item.cantidad
    );

  }


  obtenerSubtotal(): number {

    return this.items.reduce(
      (total, item) =>
        total +
        (
          item.precio *
          item.cantidad
        ),
      0
    );

  }


  limpiar(): void {

    this.items = [];

    this.emitirCambios();

  }


  private emitirCambios(): void {

    localStorage.setItem(
      this.storageKey,
      JSON.stringify(this.items)
    );

    this.itemsSubject.next(
      [...this.items]
    );

  }

}