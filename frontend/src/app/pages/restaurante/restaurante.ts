import { switchMap, forkJoin, of, catchError } from 'rxjs';
import { presentarProducto } from '../../services/imagenes-menu';
import { DestroyRef, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Auth } from "../../services/auth";
import {
  MapaUbicacion,
  PuntoEntrega,
} from "../../components/mapa-ubicacion/mapa-ubicacion";
import { ChangeDetectorRef, Component, OnInit } from "@angular/core";

import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, RouterLink } from "@angular/router";

import { Api } from "../../services/api";
import { Carrito, ItemCarrito } from "../../services/carrito";

@Component({
  selector: "app-restaurante",
  standalone: true,

  imports: [CommonModule, FormsModule, RouterLink, MapaUbicacion],

  templateUrl: "./restaurante.html",
  styleUrl: "./restaurante.scss",
})
export class Restaurante implements OnInit {
  private destroyRef = inject(DestroyRef);
  private auth = inject(Auth);
  get puedeComprar(): boolean { return !this.auth.estaAutenticado() || this.auth.tieneRol("CLIENTE"); }
  puntoEntrega: PuntoEntrega | null = null;
  ubicar(p: PuntoEntrega) {
    this.puntoEntrega = p;
  }

  restaurante: any = null;

  productos: any[] = [];
  categoriaSeleccionada='';
  get categorias():string[]{return Array.from(new Set(this.productos.map(p=>String(p.categoria||'Otros'))));}
  get productosVisibles(){return this.categoriaSeleccionada?this.productos.filter(p=>(p.categoria||'Otros')===this.categoriaSeleccionada):this.productos;}


  itemsCarrito: ItemCarrito[] = [];

  cargando = true;

  error = "";

  modalidadEntrega: "DOMICILIO" | "RECOGER" = "DOMICILIO";

  direccion = "";

  referencia = "";

  observaciones = "";

  readonly costoDelivery = 10;

  procesandoPedido = false;

  pedidoConfirmado = false;

  codigoRastreo = "";
  correoConfirmacion = "";

  mensajePedido = "";

  constructor(
    private route: ActivatedRoute,
    private api: Api,
    public carrito: Carrito,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.carrito.items$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((items) => {
        this.itemsCarrito = items;

        this.cdr.detectChanges();
      });

    this.route.paramMap.pipe(switchMap(params=>{
      this.cargando=true;this.error='';this.restaurante=null;this.productos=[];this.categoriaSeleccionada='';this.puntoEntrega=null;this.pedidoConfirmado=false;
      const id=Number(params.get('id'));
      if(!Number.isSafeInteger(id)||id<=0)return of(null);
      return forkJoin({restaurante:this.api.getRestaurante(id),productos:this.api.getProductosRestaurante(id)}).pipe(catchError(()=>of(null)));
    }),takeUntilDestroyed(this.destroyRef)).subscribe(datos=>{
      this.cargando=false;
      if(datos){this.restaurante=datos.restaurante;this.productos=datos.productos.map(presentarProducto);}
      else this.error='No se pudo cargar el restaurante y su menú.';
      this.cdr.markForCheck();
    });
  }

  cargarRestaurante(id: number): void {
    this.api.getRestaurante(id).subscribe({
      next: (datos) => {
        this.restaurante = datos;

        this.cargarProductos();
      },

      error: (error) => {
        console.error("Error restaurante:", error);

        this.error = "No se pudo cargar el restaurante.";

        this.cargando = false;

        this.cdr.detectChanges();
      },
    });
  }

  cargarProductos(): void {
    this.api.getProductosRestaurante(Number(this.restaurante.idRestaurante)).subscribe({
      next: (datos) => {
        this.productos = datos.map(presentarProducto);

        this.cargando = false;

        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error("Error productos:", error);

        this.error = "No se pudieron cargar los productos.";

        this.cargando = false;

        this.cdr.detectChanges();
      },
    });
  }

  agregar(producto: any): void {
    if(producto.disponible===false)return;
    if(!this.carrito.agregar({...producto,idRestaurante:Number(this.restaurante.idRestaurante)})) this.mensajePedido='El carrito pertenece a otro restaurante. Vacíalo antes de cambiar de restaurante.';
  }

  aumentar(idProducto: number): void {
    this.carrito.aumentar(idProducto);
  }

  disminuir(idProducto: number): void {
    this.carrito.disminuir(idProducto);
  }

  eliminar(idProducto: number): void {
    this.carrito.eliminar(idProducto);
  }

  cantidadProducto(idProducto: number): number {
    return this.carrito.obtenerCantidadProducto(idProducto);
  }

  subtotalItem(item: ItemCarrito): number {
    return (
      (Math.round(Number(item.precio) * 100) * Number(item.cantidad)) / 100
    );
  }

  subtotal(): number {
    return this.itemsCarrito.reduce(
      (total, item) =>
        total +
        Math.round(Number(item.precio) * 100) * Number(item.cantidad),

      0,
    ) / 100;
  }

  delivery(): number {
    return this.itemsCarrito.length > 0 && this.modalidadEntrega === "DOMICILIO"
      ? this.costoDelivery
      : 0;
  }

  total(): number {
    return this.subtotal() + this.delivery();
  }

  cambiarModalidad(modalidad: "DOMICILIO" | "RECOGER"): void {
    if (this.modalidadEntrega !== modalidad) this.puntoEntrega = null;
    this.modalidadEntrega = modalidad;

    if (modalidad === "RECOGER") {
      this.direccion = "";

      this.referencia = "";
    }
  }

  // ==========================================
  // CONFIRMAR PEDIDO
  // ==========================================

  confirmarPedido(): void {
    if (this.procesandoPedido) return;
    if(!this.auth.estaAutenticado()||!this.auth.tieneRol('CLIENTE')){this.mensajePedido='Inicia sesión con una cuenta de cliente para confirmar el pedido.';return;}
    if (this.modalidadEntrega === "DOMICILIO" && !this.puntoEntrega) {
      this.mensajePedido =
        "Indica la ubicación de entrega: permite el GPS o marca el punto en el mapa.";
      return;
    }

    this.mensajePedido = "";

    if (this.itemsCarrito.length === 0) {
      this.mensajePedido = "Agrega al menos un producto.";

      return;
    }

    if (this.modalidadEntrega === "DOMICILIO" && this.direccion.trim() === "") {
      this.mensajePedido = "Ingresa la dirección de entrega.";

      return;
    }

    if (!this.restaurante) {
      this.mensajePedido = "No se pudo identificar el restaurante.";

      return;
    }

    if(this.itemsCarrito.some(i=>i.idRestaurante!==Number(this.restaurante.idRestaurante))){this.mensajePedido='El carrito contiene productos de otro restaurante. Vacíalo y vuelve a elegir.';return;}
    const idCliente = this.auth.getUsuario()?.idUsuario;
    if (!idCliente) {
      this.mensajePedido =
        "Inicia sesión como cliente antes de confirmar el pedido.";
      return;
    }

    // Según los estados reales de nuestra BD:
    //
    // DOMICILIO:
    // IdEstado 1 = Pedido recibido
    //
    // RECOGER:
    // IdEstado 8 = Pedido recibido

    const idEstadoActual = this.modalidadEntrega === "DOMICILIO" ? 1 : 8;

    const pedido = {
      latitudEntrega:
        this.modalidadEntrega === "DOMICILIO"
          ? (this.puntoEntrega?.latitud ?? null)
          : null,
      longitudEntrega:
        this.modalidadEntrega === "DOMICILIO"
          ? (this.puntoEntrega?.longitud ?? null)
          : null,

      idCliente: idCliente,

      idEstadoActual: idEstadoActual,

      idRestaurante: Number(this.restaurante.idRestaurante),

      modalidadEntrega: this.modalidadEntrega,

      direccionEntrega:
        this.modalidadEntrega === "DOMICILIO" ? this.direccion : null,

      referenciaEntrega:
        this.modalidadEntrega === "DOMICILIO" ? this.referencia : null,

      costoProductos: this.subtotal(),

      costoDelivery: this.delivery(),

      total: this.total(),

      observaciones: this.observaciones,

      productos: this.itemsCarrito.map((item) => ({
        idProducto: Number(item.idProducto),

        cantidad: Number(item.cantidad),

        precioUnitario: Number(item.precio),
      })),
    };

    this.procesandoPedido = true;

    this.api.crearPedido(pedido).subscribe({
      next: (respuesta) => {

        this.codigoRastreo = respuesta.codigoRastreo;
        this.correoConfirmacion =
          respuesta.correoConfirmacion || "No configurado";

        this.pedidoConfirmado = true;

        this.procesandoPedido = false;

        this.mensajePedido = "Pedido registrado correctamente.";

        this.carrito.limpiar();

        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error("Error creando pedido:", error);

        this.procesandoPedido = false;

        this.mensajePedido =
          error?.error?.mensaje ?? "No se pudo registrar el pedido.";

        this.cdr.detectChanges();
      },
    });
  }
}
