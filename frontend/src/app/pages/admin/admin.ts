import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, from, of, catchError, finalize, map, mergeMap, switchMap, toArray } from 'rxjs';

import { Api } from '../../services/api';
import { Auth, UsuarioSesion } from '../../services/auth';

type SeccionAdmin =
  | 'dashboard'
  | 'usuarios'
  | 'servicios'
  | 'restaurantes'
  | 'motoristas';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule, RouterLink
  ],
  templateUrl: './admin.html',
  styleUrl: './admin.scss'
})
export class Admin implements OnInit {

  usuario: UsuarioSesion | null = null;

  seccion: SeccionAdmin = 'dashboard';

  dashboard: any = null;

  usuarios: any[] = [];
  roles: any[] = [];
  servicios: any[] = [];
  restaurantes: any[] = [];
  motoristas: any[] = [];
  usuariosMotoristasDisponibles: any[] = [];

  // ==========================================================
  // ASIGNACIONES
  // ==========================================================

  serviciosPendientes: any[] = [];
  motoristasAsignacion: any[] = [];

  motoristaSeleccionado: {
    [idServicio: number]: number;
  } = {};

  asignandoServicio: number | null = null;

  // ==========================================================
  // ESTADOS GENERALES
  // ==========================================================

  cargando = true;
  guardando = false;

  error = '';
  mensaje = '';

  // ==========================================================
  // USUARIO
  // ==========================================================

  mostrarFormularioUsuario = false;
  editandoUsuario = false;
  idUsuarioEditando: number | null = null;

  usuarioForm = {
    nombre: '',
    correo: '',
    password: '',
    idRol: 4
  };

  // ==========================================================
  // RESTAURANTE
  // ==========================================================

  mostrarFormularioRestaurante = false;
  editandoRestaurante = false;
  idRestauranteEditando: number | null = null;

  restauranteForm = {
    nombre: '',
    descripcion: '',
    direccion: '',
    telefono: '',
    correo: '',
    imagen: '',
    horarioApertura: '',
    horarioCierre: '',
    latitud: null as number | null,
    longitud: null as number | null
  };

  // ==========================================================
  // MOTORISTA
  // ==========================================================

  mostrarFormularioMotorista = false;
  editandoMotorista = false;
  idMotoristaEditando: number | null = null;

  motoristaForm = {
    idUsuario: 0,
    numeroLicencia: '',
    disponible: true
  };

  constructor(
    private cdr: ChangeDetectorRef,
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

    if (
      !this.usuario ||
      this.usuario.rol !== 'ADMIN_GENERAL'
    ) {

      this.router.navigate([
        '/login'
      ]);

      return;
    }

    this.cargarTodo();
  }

  // ==========================================================
  // CARGAR TODO
  // ==========================================================

  cargarTodo(): void {

    this.cargando = true;
    this.error = '';

    forkJoin({

      dashboard:
        this.api.getAdminDashboard(),

      usuarios:
        this.api.getAdminUsuarios(),

      roles:
        this.api.getAdminRoles(),

      servicios:
        this.api.getAdminServicios(),

      restaurantes:
        this.api.getAdminRestaurantes(),

      motoristas:
        this.api.getAdminMotoristas(),

      usuariosMotoristas:
        this.api.getUsuariosMotoristasDisponibles()

    })
    .pipe(finalize(() => this.cdr.markForCheck()))
    .subscribe({

      next: respuesta => {

        this.dashboard =
          respuesta.dashboard;

        this.usuarios =
          respuesta.usuarios;

        this.roles =
          respuesta.roles;

        if (!this.asignacionesListas) this.servicios = respuesta.servicios;

        this.restaurantes =
          respuesta.restaurantes;

        this.motoristas =
          respuesta.motoristas;

        this.usuariosMotoristasDisponibles =
          respuesta.usuariosMotoristas;

        this.cargando = false;
      },

      error: error => {

        console.error(
          'Error cargando administración:',
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
          'No fue posible cargar la información administrativa.';
      }
    });
  }

  // ==========================================================
  // NAVEGACIÓN
  // ==========================================================

  cambiarSeccion(
    seccion: SeccionAdmin
  ): void {

    this.seccion = seccion;

    this.error = '';
    this.mensaje = '';

    this.mostrarFormularioUsuario = false;
    this.mostrarFormularioRestaurante = false;
    this.mostrarFormularioMotorista = false;

    if (
      seccion === 'servicios'
    ) {

      this.cargarAsignaciones();
    }

    if (
      seccion === 'restaurantes'
    ) {

      this.cargarRestaurantes();
    }

    if (
      seccion === 'motoristas'
    ) {

      this.cargarMotoristas();
    }
  }

  // ==========================================================
  // ASIGNACIONES
  // ==========================================================

  cargandoAsignaciones = false;
  asignacionesListas = false;
  motoristasDisponibles: any[] = [];
  pendientesAsignables = 0;
  avisoAsignacion = '';
  tipoAvisoAsignacion: 'success' | 'error' = 'success';
  private modalidades = new Map<number, string>();
  private asignacionesConfirmadas = new Map<number, any>();

  cargarAsignaciones(trasOperacion = false): void {
    if (this.cargandoAsignaciones || (this.asignandoServicio !== null && !trasOperacion)) return;
    this.cargandoAsignaciones = true;
    this.asignacionesListas = false;
    forkJoin({
      servicios: this.api.getAdminServicios(),
      pendientes: this.api.getAsignacionPendientes(),
      motoristas: this.api.getAsignacionMotoristas()
    }).pipe(
      switchMap(respuesta => {
        const pendientes = new Map((respuesta.pendientes ?? []).map(s => [Number(s.idServicio), s]));
        const servicios = new Map<number, any>();
        for (const s of respuesta.servicios ?? []) servicios.set(Number(s.idServicio), { ...s });
        for (const [id, p] of pendientes) servicios.set(id, { ...servicios.get(id), ...p });
        const filas = Array.from(servicios.values());
        // Solo completar modalidades ausentes; máximo cuatro peticiones simultáneas.
        return from(filas).pipe(
          mergeMap(s => {
            const id = Number(s.idServicio);
            s.modalidad = s.modalidad ?? s.modalidadEntrega ?? s.pedido?.modalidadEntrega ?? this.modalidades.get(id);
            if (s.modalidad) this.modalidades.set(id, s.modalidad);
            if (this.normalizarAsignacion(s.tipoServicio) !== 'DELIVERY' || s.modalidad || this.esFinalizado(s) || !s.codigoRastreo) return of(s);
            return this.api.rastrear(s.codigoRastreo).pipe(
              map(detalle => {
                if (detalle?.modalidad) this.modalidades.set(id, detalle.modalidad);
                return { ...s, modalidad: detalle?.modalidad };
              }),
              catchError(() => of(s))
            );
          }, 4),
          toArray(),
          map(completadas => {
            const porId = new Map(completadas.map(s => [Number(s.idServicio), s]));
            return { respuesta, filas: filas.map(s => porId.get(Number(s.idServicio)) ?? s) };
          })
        );
      }),
      finalize(() => {
        this.cargandoAsignaciones = false;
        this.asignandoServicio = null;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: ({ respuesta, filas }) => {
        this.serviciosPendientes = respuesta.pendientes ?? [];
        this.motoristasAsignacion = respuesta.motoristas ?? [];
        this.servicios = filas;
        this.asignacionesListas = true;
        this.prepararAsignaciones();
      },
      error: () => {
        this.notificarAsignacion(trasOperacion
          ? 'El cambio se guardó, pero no se pudo actualizar la disponibilidad. Pulsa Actualizar antes de continuar.'
          : 'No se pudieron cargar las asignaciones. Pulsa Actualizar para reintentar.', 'error');
      }
    });
  }

  private normalizarAsignacion(valor: any): string {
    return String(valor ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim().toUpperCase();
  }

  private esFinalizado(s: any): boolean {
    const estado = this.normalizarAsignacion(s.estado ?? s.estadoActual);
    return s.activo === false || !!s.fechaFinalizacion ||
      ['ENTREGADO', 'PEDIDO ENTREGADO', 'PAQUETE ENTREGADO', 'RECOGIDO', 'PEDIDO RECOGIDO', 'FINALIZADO', 'COMPLETADO', 'CANCELADO', 'CANCELADA'].includes(estado);
  }

  private prepararAsignaciones(): void {
    const pendientes = new Set(this.serviciosPendientes.map(s => Number(s.idServicio)));
    const motoristas = new Map(this.motoristasAsignacion.map(m => [Number(m.idMotorista), m]));
    for (const s of this.servicios) {
      const id = Number(s.idServicio);
      s.idServicio = id;
      const libre = pendientes.has(id);
      if (libre) this.asignacionesConfirmadas.delete(id);
      const confirmado = this.asignacionesConfirmadas.get(id);
      const idMotorista = Number(s.idMotorista ?? s.motorista?.idMotorista ?? confirmado?.idMotorista) || null;
      const nombre = (typeof s.motorista === 'string' ? s.motorista : s.motorista?.nombre) || s.nombreMotorista || motoristas.get(idMotorista!)?.nombre || confirmado?.nombre;
      const finalizado = this.esFinalizado(s);
      const delivery = this.normalizarAsignacion(s.tipoServicio) === 'DELIVERY';
      const modalidad = this.normalizarAsignacion(s.modalidad);
      const recoger = delivery && modalidad === 'RECOGER';
      // pendientes contiene TODOS los servicios activos sin motorista (contrato del backend).
      // La ausencia solo prueba asignación cuando conocemos que el servicio sigue activo.
      const asignado = !libre && (!!idMotorista || !!nombre || s.activo === true);
      const requiere = !finalizado && !recoger && (!delivery || !!modalidad);
      s.asignacion = {
        idMotorista, asignado,
        puedeAsignar: requiere && libre,
        puedeQuitar: requiere && asignado,
        nombre: nombre || (idMotorista ? `Motorista #${idMotorista}` : 'Motorista asignado (nombre no informado por la API)'),
        etiqueta: finalizado ? 'Finalizado' : recoger ? 'No requiere motorista' : delivery && !modalidad ? 'Modalidad sin confirmar. Pulsa Actualizar.' : !libre && !asignado ? 'Asignación sin confirmar. Pulsa Actualizar.' : ''
      };
      if (!this.motoristaSeleccionado[id]) this.motoristaSeleccionado[id] = 0;
    }
    const ocupados = new Set(this.servicios.filter(s => s.asignacion.asignado && !this.esFinalizado(s)).map(s => s.asignacion.idMotorista));
    this.motoristasDisponibles = this.motoristasAsignacion.filter(m => m.activo === true && m.disponible === true && !ocupados.has(Number(m.idMotorista)));
    const disponibles = new Set(this.motoristasDisponibles.map(m => Number(m.idMotorista)));
    for (const s of this.servicios) if (!disponibles.has(Number(this.motoristaSeleccionado[s.idServicio]))) this.motoristaSeleccionado[s.idServicio] = 0;
    this.pendientesAsignables = this.servicios.filter(s => s.asignacion.puedeAsignar).length;
  }

  private notificarAsignacion(texto: string, tipo: 'success' | 'error'): void {
    this.avisoAsignacion = texto;
    this.tipoAvisoAsignacion = tipo;
    this.cdr.markForCheck();
  }

  asignarMotorista(servicio: any): void {
    if (!this.asignacionesListas || this.cargandoAsignaciones || this.asignandoServicio !== null) return;
    const fila = this.servicios.find(s => s.idServicio === Number(servicio.idServicio));
    if (!fila?.asignacion?.puedeAsignar) return;
    const motorista = this.motoristasDisponibles.find(m => Number(m.idMotorista) === Number(this.motoristaSeleccionado[fila.idServicio]));
    if (!motorista) {
      this.notificarAsignacion('Seleccione un motorista disponible.', 'error');
      return;
    }
    this.asignandoServicio = fila.idServicio;
    this.avisoAsignacion = '';
    this.api.asignarMotoristaServicio(fila.idServicio, Number(motorista.idMotorista), 'Motorista asignado desde Administración General.').subscribe({
      next: respuesta => {
        this.asignacionesConfirmadas.set(fila.idServicio, { idMotorista: Number(motorista.idMotorista), nombre: respuesta?.motorista?.nombre || motorista.nombre });
        fila.idMotorista = Number(motorista.idMotorista);
        fila.nombreMotorista = respuesta?.motorista?.nombre || motorista.nombre;
        fila.estado = respuesta?.estado || fila.estado;
        this.serviciosPendientes = this.serviciosPendientes.filter(s => Number(s.idServicio) !== fila.idServicio);
        motorista.disponible = false;
        this.prepararAsignaciones();
        this.notificarAsignacion(`${fila.codigoRastreo}: motorista asignado correctamente (${fila.nombreMotorista}).`, 'success');
        this.cargarAsignaciones(true);
      },
      error: error => {
        this.notificarAsignacion(error?.error?.mensaje || 'No fue posible asignar el motorista.', 'error');
        this.cargarAsignaciones(true);
      }
    });
  }

  quitarAsignacion(servicio: any): void {
    if (!this.asignacionesListas || this.cargandoAsignaciones || this.asignandoServicio !== null) return;
    const fila = this.servicios.find(s => s.idServicio === Number(servicio.idServicio));
    if (!fila?.asignacion?.puedeQuitar) return;
    this.asignandoServicio = fila.idServicio;
    this.avisoAsignacion = '';
    this.api.quitarMotoristaServicio(fila.idServicio).subscribe({
      next: () => {
        this.asignacionesConfirmadas.delete(fila.idServicio);
        fila.idMotorista = null;
        fila.motorista = null;
        fila.nombreMotorista = null;
        this.serviciosPendientes.push({ ...fila });
        this.prepararAsignaciones();
        this.notificarAsignacion(`${fila.codigoRastreo}: motorista retirado correctamente.`, 'success');
        // La disponibilidad se toma del servidor antes de permitir otra operación.
        this.cargarAsignaciones(true);
      },
      error: error => {
        this.notificarAsignacion(error?.error?.mensaje || 'No fue posible quitar la asignación.', 'error');
        this.cargarAsignaciones(true);
      }
    });
  }


  // ==========================================================
  // USUARIOS
  // ==========================================================

  nuevoUsuario(): void {

    this.editandoUsuario = false;
    this.idUsuarioEditando = null;

    this.usuarioForm = {
      nombre: '',
      correo: '',
      password: '',
      idRol: 4
    };

    this.mostrarFormularioUsuario = true;

    this.error = '';
    this.mensaje = '';
  }

  editarUsuario(
    usuario: any
  ): void {

    this.editandoUsuario = true;

    this.idUsuarioEditando =
      usuario.idUsuario;

    this.usuarioForm = {

      nombre:
        usuario.nombre,

      correo:
        usuario.correo,

      password:
        '',

      idRol:
        usuario.idRol
    };

    this.mostrarFormularioUsuario = true;

    this.error = '';
    this.mensaje = '';
  }

  cancelarUsuario(): void {

    this.mostrarFormularioUsuario = false;
    this.editandoUsuario = false;
    this.idUsuarioEditando = null;
  }

  guardarUsuario(): void {

    this.error = '';
    this.mensaje = '';

    if (
      !this.usuarioForm.nombre.trim() ||
      !this.usuarioForm.correo.trim()
    ) {

      this.error =
        'Nombre y correo son obligatorios.';

      return;
    }

    if (
      !this.editandoUsuario &&
      !this.usuarioForm.password
    ) {

      this.error =
        'Ingrese una contraseña para el nuevo usuario.';

      return;
    }

    this.guardando = true;

    if (
      this.editandoUsuario &&
      this.idUsuarioEditando
    ) {

      const datos = {

        nombre:
          this.usuarioForm.nombre.trim(),

        correo:
          this.usuarioForm.correo
            .trim()
            .toLowerCase(),

        idRol:
          Number(
            this.usuarioForm.idRol
          ),

        password:
          this.usuarioForm.password ||
          null
      };

      this.api
        .editarAdminUsuario(
          this.idUsuarioEditando,
          datos
        )
        .subscribe({

          next: respuesta => {

            this.guardando = false;

            this.mensaje =
              respuesta.mensaje;

            this.cancelarUsuario();
            this.cargarUsuarios();
          },

          error: error => {

            this.guardando = false;

            this.error =
              error?.error?.mensaje ??
              'No se pudo actualizar el usuario.';
          }
        });

      return;
    }

    const datos = {

      nombre:
        this.usuarioForm.nombre
          .trim(),

      correo:
        this.usuarioForm.correo
          .trim()
          .toLowerCase(),

      password:
        this.usuarioForm.password,

      idRol:
        Number(
          this.usuarioForm.idRol
        )
    };

    this.api
      .crearAdminUsuario(
        datos
      )
      .subscribe({

        next: respuesta => {

          this.guardando = false;

          this.mensaje =
            respuesta.mensaje;

          this.cancelarUsuario();
          this.cargarUsuarios();
        },

        error: error => {

          this.guardando = false;

          this.error =
            error?.error?.mensaje ??
            'No se pudo crear el usuario.';
        }
      });
  }

  cambiarEstadoUsuario(
    usuario: any
  ): void {

    if (
      usuario.idUsuario ===
      this.usuario?.idUsuario
    ) {

      this.error =
        'No puedes desactivar tu propia cuenta mientras tienes la sesión iniciada.';

      return;
    }

    const nuevoEstado =
      !usuario.activo;

    this.api
      .cambiarEstadoAdminUsuario(
        usuario.idUsuario,
        nuevoEstado
      )
      .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta.mensaje;

          this.error = '';

          this.cargarUsuarios();
        },

        error: error => {

          this.error =
            error?.error?.mensaje ??
            'No fue posible cambiar el estado.';
        }
      });
  }

  cargarUsuarios(): void {

    this.api
      .getAdminUsuarios()
      .subscribe({

        next: datos => {

          this.usuarios =
            datos;

          this.api
            .getAdminDashboard()
            .subscribe({

              next: dashboard => {

                this.dashboard =
                  dashboard;
              }
            });
        },

        error: error => {

          console.error(
            error
          );
        }
      });
  }

  // ==========================================================
  // RESTAURANTES
  // ==========================================================

  cargarRestaurantes(): void {

    this.api
      .getAdminRestaurantes()
      .subscribe({

        next: datos => {

          this.restaurantes =
            datos;
        },

        error: error => {

          console.error(
            'Error restaurantes:',
            error
          );

          this.error =
            error?.error?.mensaje ??
            'No se pudieron cargar los restaurantes.';
        }
      });
  }

  nuevoRestaurante(): void {

    this.editandoRestaurante = false;
    this.idRestauranteEditando = null;

    this.restauranteForm = {
      nombre: '',
      descripcion: '',
      direccion: '',
      telefono: '',
      correo: '',
      imagen: '',
      horarioApertura: '',
      horarioCierre: '',
      latitud: null,
      longitud: null
    };

    this.mostrarFormularioRestaurante = true;

    this.error = '';
    this.mensaje = '';
  }

  editarRestaurante(
    restaurante: any
  ): void {

    this.editandoRestaurante = true;

    this.idRestauranteEditando =
      restaurante.idRestaurante;

    this.restauranteForm = {

      nombre:
        restaurante.nombre ?? '',

      descripcion:
        restaurante.descripcion ?? '',

      direccion:
        restaurante.direccion ?? '',

      telefono:
        restaurante.telefono ?? '',

      correo:
        restaurante.correo ?? '',

      imagen:
        restaurante.imagen ?? '',

      horarioApertura:
        this.formatearHora(
          restaurante.horarioApertura
        ),

      horarioCierre:
        this.formatearHora(
          restaurante.horarioCierre
        ),

      latitud:
        restaurante.latitud ?? null,

      longitud:
        restaurante.longitud ?? null
    };

    this.mostrarFormularioRestaurante = true;

    this.error = '';
    this.mensaje = '';
  }

  cancelarRestaurante(): void {

    this.mostrarFormularioRestaurante = false;
    this.editandoRestaurante = false;
    this.idRestauranteEditando = null;
  }

  guardarRestaurante(): void {

    this.error = '';
    this.mensaje = '';

    if (
      !this.restauranteForm.nombre.trim() ||
      !this.restauranteForm.direccion.trim()
    ) {

      this.error =
        'Nombre y dirección son obligatorios.';

      return;
    }

    const datos = {

      nombre:
        this.restauranteForm.nombre.trim(),

      descripcion:
        this.valorTexto(
          this.restauranteForm.descripcion
        ),

      direccion:
        this.restauranteForm.direccion.trim(),

      telefono:
        this.valorTexto(
          this.restauranteForm.telefono
        ),

      correo:
        this.valorTexto(
          this.restauranteForm.correo
        )?.toLowerCase(),

      imagen:
        this.valorTexto(
          this.restauranteForm.imagen
        ),

      horarioApertura:
        this.valorHora(
          this.restauranteForm.horarioApertura
        ),

      horarioCierre:
        this.valorHora(
          this.restauranteForm.horarioCierre
        ),

      latitud:
        this.restauranteForm.latitud,

      longitud:
        this.restauranteForm.longitud
    };

    this.guardando = true;

    if (
      this.editandoRestaurante &&
      this.idRestauranteEditando
    ) {

      this.api
        .editarAdminRestaurante(
          this.idRestauranteEditando,
          datos
        )
        .subscribe({

          next: respuesta => {

            this.guardando = false;

            this.mensaje =
              respuesta.mensaje;

            this.cancelarRestaurante();
            this.cargarRestaurantes();
          },

          error: error => {

            this.guardando = false;

            this.error =
              error?.error?.mensaje ??
              'No se pudo actualizar el restaurante.';
          }
        });

      return;
    }

    this.api
      .crearAdminRestaurante(
        datos
      )
      .subscribe({

        next: respuesta => {

          this.guardando = false;

          this.mensaje =
            respuesta.mensaje;

          this.cancelarRestaurante();
          this.cargarRestaurantes();
        },

        error: error => {

          this.guardando = false;

          this.error =
            error?.error?.mensaje ??
            'No se pudo crear el restaurante.';
        }
      });
  }

  cambiarEstadoRestaurante(
    restaurante: any
  ): void {

    this.api
      .cambiarEstadoAdminRestaurante(
        restaurante.idRestaurante,
        !restaurante.activo
      )
      .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta.mensaje;

          this.error = '';

          this.cargarRestaurantes();
        },

        error: error => {

          this.error =
            error?.error?.mensaje ??
            'No se pudo cambiar el estado del restaurante.';
        }
      });
  }

  // ==========================================================
  // MOTORISTAS
  // ==========================================================

  cargarMotoristas(): void {

    forkJoin({

      motoristas:
        this.api.getAdminMotoristas(),

      usuarios:
        this.api.getUsuariosMotoristasDisponibles()

    })
    .subscribe({

      next: respuesta => {

        this.motoristas =
          respuesta.motoristas;

        this.usuariosMotoristasDisponibles =
          respuesta.usuarios;
      },

      error: error => {

        console.error(
          'Error motoristas:',
          error
        );

        this.error =
          error?.error?.mensaje ??
          'No se pudieron cargar los motoristas.';
      }
    });
  }

  nuevoMotorista(): void {

    this.editandoMotorista = false;
    this.idMotoristaEditando = null;

    this.motoristaForm = {
      idUsuario: 0,
      numeroLicencia: '',
      disponible: true
    };

    this.mostrarFormularioMotorista = true;

    this.error = '';
    this.mensaje = '';

    this.api
      .getUsuariosMotoristasDisponibles()
      .subscribe({

        next: datos => {

          this.usuariosMotoristasDisponibles =
            datos;
        }
      });
  }

  editarMotorista(
    motorista: any
  ): void {

    this.editandoMotorista = true;

    this.idMotoristaEditando =
      motorista.idMotorista;

    this.motoristaForm = {

      idUsuario:
        motorista.idUsuario,

      numeroLicencia:
        motorista.numeroLicencia ?? '',

      disponible:
        motorista.disponible
    };

    this.mostrarFormularioMotorista = true;

    this.error = '';
    this.mensaje = '';
  }

  cancelarMotorista(): void {

    this.mostrarFormularioMotorista = false;
    this.editandoMotorista = false;
    this.idMotoristaEditando = null;
  }

  guardarMotorista(): void {

    this.error = '';
    this.mensaje = '';

    if (
      !this.editandoMotorista &&
      Number(
        this.motoristaForm.idUsuario
      ) <= 0
    ) {

      this.error =
        'Seleccione un usuario con rol MOTORISTA.';

      return;
    }

    this.guardando = true;

    if (
      this.editandoMotorista &&
      this.idMotoristaEditando
    ) {

      const datos = {

        numeroLicencia:
          this.valorTexto(
            this.motoristaForm.numeroLicencia
          ),

        disponible:
          this.motoristaForm.disponible
      };

      this.api
        .editarAdminMotorista(
          this.idMotoristaEditando,
          datos
        )
        .subscribe({

          next: respuesta => {

            this.guardando = false;

            this.mensaje =
              respuesta.mensaje;

            this.cancelarMotorista();
            this.cargarMotoristas();
          },

          error: error => {

            this.guardando = false;

            this.error =
              error?.error?.mensaje ??
              'No se pudo actualizar el motorista.';
          }
        });

      return;
    }

    const datos = {

      idUsuario:
        Number(
          this.motoristaForm.idUsuario
        ),

      numeroLicencia:
        this.valorTexto(
          this.motoristaForm.numeroLicencia
        ),

      disponible:
        this.motoristaForm.disponible
    };

    this.api
      .crearAdminMotorista(
        datos
      )
      .subscribe({

        next: respuesta => {

          this.guardando = false;

          this.mensaje =
            respuesta.mensaje;

          this.cancelarMotorista();
          this.cargarMotoristas();
        },

        error: error => {

          this.guardando = false;

          this.error =
            error?.error?.mensaje ??
            'No se pudo registrar el motorista.';
        }
      });
  }

  cambiarEstadoMotorista(
    motorista: any
  ): void {

    this.api
      .cambiarEstadoAdminMotorista(
        motorista.idMotorista,
        !motorista.activo
      )
      .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta.mensaje;

          this.error = '';

          this.cargarMotoristas();
        },

        error: error => {

          this.error =
            error?.error?.mensaje ??
            'No se pudo cambiar el estado del motorista.';
        }
      });
  }

  cambiarDisponibilidadMotorista(
    motorista: any
  ): void {

    this.api
      .cambiarDisponibilidadAdminMotorista(
        motorista.idMotorista,
        !motorista.disponible
      )
      .subscribe({

        next: respuesta => {

          this.mensaje =
            respuesta.mensaje;

          this.error = '';

          this.cargarMotoristas();
        },

        error: error => {

          this.error =
            error?.error?.mensaje ??
            'No se pudo cambiar la disponibilidad.';
        }
      });
  }

  // ==========================================================
  // UTILIDADES
  // ==========================================================

  private valorTexto(
    valor: string | null | undefined
  ): string | null {

    if (
      !valor ||
      !valor.trim()
    ) {

      return null;
    }

    return valor.trim();
  }

  private valorHora(
    valor: string
  ): string | null {

    if (
      !valor
    ) {

      return null;
    }

    return valor.length === 5
      ? `${valor}:00`
      : valor;
  }

  private formatearHora(
    valor: string | null
  ): string {

    if (
      !valor
    ) {

      return '';
    }

    return valor.substring(
      0,
      5
    );
  }

  cerrarSesion(): void {

    this.auth.logout();

    this.router.navigate([
      '/login'
    ]);
  }
}
