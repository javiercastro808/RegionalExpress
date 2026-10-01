import { inject, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { Api } from '../../services/api';
import { Auth, UsuarioSesion } from '../../services/auth';

@Component({
  selector: 'app-admin-restaurante',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './admin-restaurante.html',
  styleUrl: './admin-restaurante.scss'
})
export class AdminRestaurante implements OnInit {
 private cdr=inject(ChangeDetectorRef);private destroyRef=inject(DestroyRef);

  usuario: UsuarioSesion | null = null;

  seccion:
    'inicio' |
    'pedidos' |
    'productos' |
    'categorias' |
    'historial'
    = 'inicio';

  dashboard: any = null;

  pedidos: any[] = [];
  productos: any[] = [];
  categorias: any[] = [];
  historial: any[] = [];

  pedidoSeleccionado: any = null;

  cargando = true;
  guardando = false;

  error = '';
  mensaje = '';

  // ==========================================================
  // PRODUCTO
  // ==========================================================

  mostrarFormularioProducto = false;
  editandoProducto = false;
  idProductoEditando: number | null = null;

  productoForm = {
    idCategoria: 0,
    nombre: '',
    descripcion: '',
    precio: 0,
    imagen: '',
    disponible: true
  };

  // ==========================================================
  // CATEGORÍA
  // ==========================================================

  mostrarFormularioCategoria = false;
  editandoCategoria = false;
  idCategoriaEditando: number | null = null;

  categoriaForm = {
    nombre: '',
    descripcion: ''
  };

  constructor(
    private api: Api,
    private auth: Auth,
    private router: Router
  ) {}

  // ==========================================================
  // INICIO
  // ==========================================================

  ngOnInit(): void {

    this.usuario =
      this.auth.getUsuario();

    if (!this.usuario) {

      this.router.navigate([
        '/login'
      ]);

      return;
    }

    const rol =
      String(
        this.usuario.rol ?? ''
      ).toUpperCase();

    if (
      rol !== 'ADMIN_RESTAURANTE' &&
      rol !== 'ADMINISTRADOR_RESTAURANTE'
    ) {

      this.router.navigate([
        '/login'
      ]);

      return;
    }

    this.cargarTodo();
  }

  // ==========================================================
  // CARGA GENERAL
  // ==========================================================

  cargarTodo(): void {

    this.cargando = true;

    this.error = '';

    forkJoin({

      dashboard:
        this.api
          .getRestauranteAdminDashboard(),

      pedidos:
        this.api
          .getRestauranteAdminPedidos(),

      productos:
        this.api
          .getRestauranteAdminProductos(),

      categorias:
        this.api
          .getRestauranteAdminCategorias(),
      historial:this.api.getRestauranteAdminHistorial()
    })
    .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

      next: respuesta => {

        this.dashboard =
          respuesta.dashboard;

        this.pedidos =
          respuesta.pedidos ?? [];

        this.productos =
          respuesta.productos ?? [];

        this.categorias =
          respuesta.categorias ?? [];

        this.historial = respuesta.historial ?? [];

        this.cargando = false;
      },

      error: error => {

        console.error(
          'Error Admin Restaurante:',
          error
        );

        this.cargando = false;

        if (
          error.status === 401 ||
          error.status === 403
        ) {

          this.auth.logout();

          this.router.navigate([
            '/login'
          ]);

          return;
        }

        this.error =
          error?.error?.mensaje ??
          'No fue posible cargar la información del restaurante.';
      }
    });
  }

  // ==========================================================
  // NAVEGACIÓN
  // ==========================================================

  cambiarSeccion(
    seccion:
      'inicio' |
      'pedidos' |
      'productos' |
      'categorias' |
      'historial'
  ): void {

    this.seccion =
      seccion;

    this.error = '';
    this.mensaje = '';

    this.pedidoSeleccionado =
      null;
  }

  // ==========================================================
  // PEDIDOS
  // ==========================================================

  cargarPedidos(): void {

    this.api
      .getRestauranteAdminPedidos()
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: (datos: any[]) => {

          this.pedidos =
            datos ?? [];
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No fue posible cargar los pedidos.';
        }
      });
  }

  verPedido(
    pedido: any
  ): void {

    const idPedido =
      Number(
        pedido.idPedido
      );

    if (!idPedido) {
      return;
    }

    this.error = '';
    this.mensaje = '';

    this.api
      .getRestauranteAdminPedido(
        idPedido
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: datos => {

          this.pedidoSeleccionado =
            datos;
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No se pudo obtener el detalle del pedido.';
        }
      });
  }

  cerrarDetallePedido(): void {

    this.pedidoSeleccionado =
      null;
  }

  cambiarEstadoPedido(
    pedido: any,
    event: Event
  ): void {

    const select =
      event.target as HTMLSelectElement;

    const idEstado =
      Number(
        select.value
      );

    if (!idEstado) {
      return;
    }

    this.guardando = true;

    this.error = '';
    this.mensaje = '';

    this.api
      .cambiarEstadoPedidoRestaurante(
        Number(
          pedido.idPedido
        ),
        idEstado,
        'Estado actualizado desde panel del restaurante'
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.guardando =
            false;

          this.mensaje =
            respuesta?.mensaje ??
            'Estado actualizado correctamente.';

          this.cargarPedidos();

          this.cargarDashboard();
        },

        error: error => {

          console.error(error);

          this.guardando =
            false;

          this.error =
            error?.error?.mensaje ??
            'No fue posible actualizar el estado.';
        }
      });
  }

  // ==========================================================
  // PRODUCTOS
  // ==========================================================

  nuevoProducto(): void {

    this.editandoProducto =
      false;

    this.idProductoEditando =
      null;

    this.productoForm = {

      idCategoria:
        this.categorias.length > 0
          ? Number(
              this.categorias[0]
                .idCategoria
            )
          : 0,

      nombre: '',

      descripcion: '',

      precio: 0,

      imagen: '',

      disponible: true
    };

    this.mostrarFormularioProducto =
      true;

    this.error = '';
    this.mensaje = '';
  }

  editarProducto(
    producto: any
  ): void {

    this.editandoProducto =
      true;

    this.idProductoEditando =
      Number(
        producto.idProducto
      );

    this.productoForm = {

      idCategoria:
        Number(
          producto.idCategoria
        ),

      nombre:
        producto.nombre ?? '',

      descripcion:
        producto.descripcion ?? '',

      precio:
        Number(
          producto.precio ?? 0
        ),

      imagen:
        producto.imagen ?? '',

      disponible:
        Boolean(
          producto.disponible
        )
    };

    this.mostrarFormularioProducto =
      true;

    this.error = '';
    this.mensaje = '';
  }

  cancelarProducto(): void {

    this.mostrarFormularioProducto =
      false;

    this.editandoProducto =
      false;

    this.idProductoEditando =
      null;
  }

  guardarProducto(): void {

    this.error = '';
    this.mensaje = '';

    if (
      !this.productoForm
        .nombre
        .trim()
    ) {

      this.error =
        'El nombre del producto es obligatorio.';

      return;
    }

    if (
      Number(
        this.productoForm
          .idCategoria
      ) <= 0
    ) {

      this.error =
        'Seleccione una categoría.';

      return;
    }

    if (
      Number(
        this.productoForm
          .precio
      ) <= 0
    ) {

      this.error =
        'El precio debe ser mayor que cero.';

      return;
    }

    const datos = {

      idCategoria:
        Number(
          this.productoForm
            .idCategoria
        ),

      nombre:
        this.productoForm
          .nombre
          .trim(),

      descripcion:
        this.productoForm
          .descripcion
          .trim(),

      precio:
        Number(
          this.productoForm
            .precio
        ),

      imagen:
        this.productoForm
          .imagen
          .trim() || null,

      disponible:
        this.productoForm
          .disponible
    };

    this.guardando = true;

    if (
      this.editandoProducto &&
      this.idProductoEditando
    ) {

      this.api
        .editarRestauranteAdminProducto(
          this.idProductoEditando,
          datos
        )
        .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

          next: respuesta => {

            this.guardando =
              false;

            this.mensaje =
              respuesta?.mensaje ??
              'Producto actualizado.';

            this.cancelarProducto();

            this.cargarProductos();

            this.cargarDashboard();
          },

          error: error => {

            console.error(error);

            this.guardando =
              false;

            this.error =
              error?.error?.mensaje ??
              'No fue posible actualizar el producto.';
          }
        });

      return;
    }

    this.api
      .crearRestauranteAdminProducto(
        datos
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.guardando =
            false;

          this.mensaje =
            respuesta?.mensaje ??
            'Producto creado correctamente.';

          this.cancelarProducto();

          this.cargarProductos();

          this.cargarDashboard();
        },

        error: error => {

          console.error(error);

          this.guardando =
            false;

          this.error =
            error?.error?.mensaje ??
            'No fue posible crear el producto.';
        }
      });
  }

  cambiarDisponibilidadProducto(
    producto: any
  ): void {

    const disponible =
      !Boolean(
        producto.disponible
      );

    this.error = '';
    this.mensaje = '';

    this.api
      .cambiarDisponibilidadRestauranteAdminProducto(
        Number(
          producto.idProducto
        ),
        disponible
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta?.mensaje ??
            'Disponibilidad actualizada.';

          this.cargarProductos();
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No fue posible cambiar la disponibilidad.';
        }
      });
  }

  cambiarEstadoProducto(
    producto: any
  ): void {

    const activo =
      !Boolean(
        producto.activo
      );

    this.error = '';
    this.mensaje = '';

    this.api
      .cambiarEstadoRestauranteAdminProducto(
        Number(
          producto.idProducto
        ),
        activo
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta?.mensaje ??
            'Estado del producto actualizado.';

          this.cargarProductos();

          this.cargarDashboard();
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No fue posible cambiar el estado del producto.';
        }
      });
  }

  cargarProductos(): void {

    this.api
      .getRestauranteAdminProductos()
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: (datos: any[]) => {

          this.productos =
            datos ?? [];
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No fue posible cargar los productos.';
        }
      });
  }

  // ==========================================================
  // CATEGORÍAS
  // ==========================================================

  nuevaCategoria(): void {

    this.editandoCategoria =
      false;

    this.idCategoriaEditando =
      null;

    this.categoriaForm = {

      nombre: '',

      descripcion: ''
    };

    this.mostrarFormularioCategoria =
      true;

    this.error = '';
    this.mensaje = '';
  }

  editarCategoria(
    categoria: any
  ): void {

    this.editandoCategoria =
      true;

    this.idCategoriaEditando =
      Number(
        categoria.idCategoria
      );

    this.categoriaForm = {

      nombre:
        categoria.nombre ?? '',

      descripcion:
        categoria.descripcion ?? ''
    };

    this.mostrarFormularioCategoria =
      true;

    this.error = '';
    this.mensaje = '';
  }

  cancelarCategoria(): void {

    this.mostrarFormularioCategoria =
      false;

    this.editandoCategoria =
      false;

    this.idCategoriaEditando =
      null;
  }

  guardarCategoria(): void {

    this.error = '';
    this.mensaje = '';

    if (
      !this.categoriaForm
        .nombre
        .trim()
    ) {

      this.error =
        'El nombre de la categoría es obligatorio.';

      return;
    }

    const datos = {

      nombre:
        this.categoriaForm
          .nombre
          .trim(),

      descripcion:
        this.categoriaForm
          .descripcion
          .trim()
    };

    this.guardando = true;

    if (
      this.editandoCategoria &&
      this.idCategoriaEditando
    ) {

      this.api
        .editarRestauranteAdminCategoria(
          this.idCategoriaEditando,
          datos
        )
        .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

          next: respuesta => {

            this.guardando =
              false;

            this.mensaje =
              respuesta?.mensaje ??
              'Categoría actualizada.';

            this.cancelarCategoria();

            this.cargarCategorias();

            this.cargarDashboard();
          },

          error: error => {

            console.error(error);

            this.guardando =
              false;

            this.error =
              error?.error?.mensaje ??
              'No fue posible actualizar la categoría.';
          }
        });

      return;
    }

    this.api
      .crearRestauranteAdminCategoria(
        datos
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.guardando =
            false;

          this.mensaje =
            respuesta?.mensaje ??
            'Categoría creada correctamente.';

          this.cancelarCategoria();

          this.cargarCategorias();

          this.cargarDashboard();
        },

        error: error => {

          console.error(error);

          this.guardando =
            false;

          this.error =
            error?.error?.mensaje ??
            'No fue posible crear la categoría.';
        }
      });
  }

  cambiarEstadoCategoria(
    categoria: any
  ): void {

    const activo =
      !Boolean(
        categoria.activo
      );

    this.error = '';
    this.mensaje = '';

    this.api
      .cambiarEstadoRestauranteAdminCategoria(
        Number(
          categoria.idCategoria
        ),
        activo
      )
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta?.mensaje ??
            'Estado de categoría actualizado.';

          this.cargarCategorias();

          this.cargarDashboard();
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No fue posible cambiar el estado.';
        }
      });
  }

  cargarCategorias(): void {

    this.api
      .getRestauranteAdminCategorias()
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: (datos: any[]) => {

          this.categorias =
            datos ?? [];
        },

        error: error => {

          console.error(error);

          this.error =
            error?.error?.mensaje ??
            'No fue posible cargar las categorías.';
        }
      });
  }

  // ==========================================================
  // HISTORIAL
  // ==========================================================

  cargarHistorial(): void {

    // El controlador independiente de historial
    // fue eliminado.
    //
    // Conservamos el método porque el HTML actual
    // puede seguir llamándolo.

    this.historial = [];
  }

  // ==========================================================
  // DASHBOARD
  // ==========================================================

  cargarDashboard(): void {

    this.api
      .getRestauranteAdminDashboard()
      .pipe(takeUntilDestroyed(this.destroyRef),finalize(()=>this.cdr.markForCheck()))
    .subscribe({

        next: datos => {

          this.dashboard =
            datos;
        },

        error: error => {

          console.error(error);
        }
      });
  }

  // ==========================================================
  // LOGOUT
  // ==========================================================

  cerrarSesion(): void {

    this.auth.logout();

    this.router.navigate([
      '/login'
    ]);
  }
}