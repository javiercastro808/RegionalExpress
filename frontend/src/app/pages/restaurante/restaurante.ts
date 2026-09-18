import { Auth } from "../../services/auth";
import { presentarProducto } from '../../services/imagenes-menu';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { Api } from '../../services/api';
import {
  Carrito,
  ItemCarrito
} from '../../services/carrito';

@Component({
  selector: 'app-restaurante',
  standalone: true,

  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],

  templateUrl: './restaurante.html',
  styleUrl: './restaurante.scss'
})
export class Restaurante implements OnInit {

  restaurante: any = null;

  productos: any[] = [];

  itemsCarrito: ItemCarrito[] = [];

  cargando = true;

  error = '';

  modalidadEntrega: 'DOMICILIO' | 'RECOGER' =
    'DOMICILIO';

  direccion = '';

  referencia = '';

  observaciones = '';

  readonly costoDelivery = 10;

  procesandoPedido = false;

  pedidoConfirmado = false;

  codigoRastreo = '';
  correoConfirmacion = ''; 

  mensajePedido = '';


  constructor(
    private auth: Auth,
    private route: ActivatedRoute,
    private api: Api,
    public carrito: Carrito,
    private cdr: ChangeDetectorRef
  ) {}


  ngOnInit(): void {

    this.carrito.items$
      .subscribe(items => {

        this.itemsCarrito = items;

        this.cdr.detectChanges();

      });


    const id =
      Number(
        this.route.snapshot.paramMap.get('id')
      );


    if (!id) {

      this.error =
        'Restaurante no válido.';

      this.cargando = false;

      return;

    }


    this.cargarRestaurante(id);

  }


  cargarRestaurante(
    id: number
  ): void {

    this.api
      .getRestaurante(id)
      .subscribe({

        next: datos => {

          this.restaurante = datos;

          this.cargarProductos();

        },

        error: error => {

          console.error(
            'Error restaurante:',
            error
          );

          this.error =
            'No se pudo cargar el restaurante.';

          this.cargando = false;

          this.cdr.detectChanges();

        }

      });

  }


  cargarProductos(): void {

    this.api
      .getProductos()
      .subscribe({

        next: datos => {

          this.productos = datos.map(presentarProducto);

          this.cargando = false;

          this.cdr.detectChanges();

        },

        error: error => {

          console.error(
            'Error productos:',
            error
          );

          this.error =
            'No se pudieron cargar los productos.';

          this.cargando = false;

          this.cdr.detectChanges();

        }

      });

  }


  agregar(
    producto: any
  ): void {

    if (producto.disponible === false) return;
    this.carrito.agregar(
      producto
    );

  }


  aumentar(
    idProducto: number
  ): void {

    this.carrito.aumentar(
      idProducto
    );

  }


  disminuir(
    idProducto: number
  ): void {

    this.carrito.disminuir(
      idProducto
    );

  }


  eliminar(
    idProducto: number
  ): void {

    this.carrito.eliminar(
      idProducto
    );

  }


  cantidadProducto(
    idProducto: number
  ): number {

    return this.carrito
      .obtenerCantidadProducto(
        idProducto
      );

  }


  subtotalItem(
    item: ItemCarrito
  ): number {

    return (
      Number(item.precio) *
      Number(item.cantidad)
    );

  }


  subtotal(): number {

    return this.itemsCarrito.reduce(

      (total, item) =>
        total +
        (
          Number(item.precio) *
          Number(item.cantidad)
        ),

      0

    );

  }


  delivery(): number {

    return (
      this.modalidadEntrega ===
      'DOMICILIO'
        ? this.costoDelivery
        : 0
    );

  }


  total(): number {

    return (
      this.subtotal() +
      this.delivery()
    );

  }


  cambiarModalidad(
    modalidad:
      'DOMICILIO' |
      'RECOGER'
  ): void {

    this.modalidadEntrega =
      modalidad;


    if (
      modalidad ===
      'RECOGER'
    ) {

      this.direccion = '';

      this.referencia = '';

    }

  }


  // ==========================================
  // CONFIRMAR PEDIDO
  // ==========================================

  confirmarPedido(): void {
    if (this.procesandoPedido) return;
    if (!this.auth.estaAutenticado() || !this.auth.tieneRol("CLIENTE")) { this.mensajePedido = "Inicia sesión con una cuenta de cliente para confirmar tu pedido."; return; }

    this.mensajePedido = '';


    if (
      this.itemsCarrito.length === 0
    ) {

      this.mensajePedido =
        'Agrega al menos un producto.';

      return;

    }


    if (
      this.modalidadEntrega ===
      'DOMICILIO' &&
      this.direccion.trim() === ''
    ) {

      this.mensajePedido =
        'Ingresa la dirección de entrega.';

      return;

    }


    if (!this.restaurante) {

      this.mensajePedido =
        'No se pudo identificar el restaurante.';

      return;

    }


    const idCliente = this.auth.getUsuario()!.idUsuario;


    // Según los estados reales de nuestra BD:
    //
    // DOMICILIO:
    // IdEstado 1 = Pedido recibido
    //
    // RECOGER:
    // IdEstado 8 = Pedido recibido

    const idEstadoActual =
      this.modalidadEntrega ===
      'DOMICILIO'
        ? 1
        : 8;


    const pedido = {

      idCliente:
        idCliente,

      idEstadoActual:
        idEstadoActual,

      idRestaurante:
        Number(
          this.restaurante.idRestaurante
        ),

      modalidadEntrega:
        this.modalidadEntrega,

      direccionEntrega:
        this.modalidadEntrega ===
        'DOMICILIO'
          ? this.direccion
          : null,

      referenciaEntrega:
        this.modalidadEntrega ===
        'DOMICILIO'
          ? this.referencia
          : null,

      costoProductos:
        this.subtotal(),

      costoDelivery:
        this.delivery(),

      total:
        this.total(),

      observaciones:
        this.observaciones,

      productos:
        this.itemsCarrito.map(
          item => ({
            idProducto:
              Number(
                item.idProducto
              ),

            cantidad:
              Number(
                item.cantidad
              ),

            precioUnitario:
              Number(
                item.precio
              )
          })
        )

    };


    console.log(
      'Pedido enviado:',
      pedido
    );


    this.procesandoPedido =
      true;


    this.api
      .crearPedido(pedido)
      .subscribe({

        next: respuesta => {

          console.log(
            'Pedido creado:',
            respuesta
          );


          this.codigoRastreo =
            respuesta.codigoRastreo;
          this.correoConfirmacion = respuesta.correoConfirmacion || 'No configurado';


          this.pedidoConfirmado =
            true;


          this.procesandoPedido =
            false;


          this.mensajePedido =
            'Pedido registrado correctamente.';


          this.carrito.limpiar();


          this.cdr.detectChanges();

        },


        error: error => {

          console.error(
            'Error creando pedido:',
            error
          );


          this.procesandoPedido =
            false;


          this.mensajePedido =
            error?.error?.mensaje
            ??
            'No se pudo registrar el pedido.';


          this.cdr.detectChanges();

        }

      });

  }

}