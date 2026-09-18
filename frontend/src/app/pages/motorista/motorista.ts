import { ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';

import { Api } from '../../services/api';
import { Auth, UsuarioSesion } from '../../services/auth';

@Component({
  selector: 'app-motorista',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule, RouterLink
  ],
  templateUrl: './motorista.html',
  styleUrl: './motorista.scss'
})
export class Motorista implements OnInit, OnDestroy {

  usuario: UsuarioSesion | null = null;

  seccion:
    'inicio' |
    'servicios' |
    'delivery' |
    'envios' |
    'historial'
    = 'inicio';

  dashboard: any = null;

  servicios: any[] = [];

  historial: any[] = [];

  servicioSeleccionado: any = null;

  estadosServicio: any[] = [];

  idEstadoSeleccionado = 0;

  observacion = '';

  cargando = true;

  cargandoDetalle = false;

  guardando = false;

  mensaje = '';

  error = '';

  constructor(
    private cdr: ChangeDetectorRef,
    private api: Api,
    private auth: Auth,
    private router: Router
  ) {}

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

    if (rol !== 'MOTORISTA') {

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
        this.api.getMotoristaDashboard(),

      servicios:
        this.api.getMotoristaServicios(),

      historial:
        this.api.getMotoristaHistorial()
    })
    .pipe(finalize(() => this.cdr.markForCheck()))
    .subscribe({

      next: respuesta => {

        this.dashboard =
          respuesta.dashboard;

        this.servicios =
          respuesta.servicios ?? [];

        this.historial =
          respuesta.historial ?? [];

        this.cargando = false;
      },

      error: error => {

        console.error(error);

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
          'No fue posible cargar el panel del motorista.';
      }
    });
  }

  // ==========================================================
  // NAVEGACIÓN INTERNA
  // ==========================================================

  cambiarSeccion(
    seccion:
      'inicio' |
      'servicios' |
      'delivery' |
      'envios' |
      'historial'
  ): void {

    this.seccion = seccion;

    this.error = '';

    this.mensaje = '';

    this.cerrarDetalle();
  }

  // ==========================================================
  // FILTROS
  // ==========================================================

  get serviciosActivos(): any[] {

    return this.servicios.filter(
      servicio =>
        servicio.activo &&
        !servicio.fechaFinalizacion
    );
  }

  get serviciosDelivery(): any[] {

    return this.servicios.filter(
      servicio =>
        String(
          servicio.tipoServicio
        ).toUpperCase() ===
        'DELIVERY'
    );
  }

  get serviciosEnvio(): any[] {

    return this.servicios.filter(
      servicio =>
        String(
          servicio.tipoServicio
        ).toUpperCase() ===
        'ENVIO'
    );
  }

  get serviciosVista(): any[] {

    if (
      this.seccion ===
      'delivery'
    ) {

      return this.serviciosDelivery;
    }

    if (
      this.seccion ===
      'envios'
    ) {

      return this.serviciosEnvio;
    }

    return this.servicios;
  }

  // ==========================================================
  // DETALLE DEL SERVICIO
  // ==========================================================

  verServicio(
    servicio: any
  ): void {

    const idServicio =
      Number(
        servicio.idServicio
      );

    if (!idServicio) {
      return;
    }

    this.cargandoDetalle = true;

    this.error = '';

    this.mensaje = '';

    forkJoin({
      detalle:
        this.api.getMotoristaServicio(
          idServicio
        ),

      estados:
        this.api.getMotoristaEstados(
          idServicio
        )
    })
    .pipe(finalize(() => this.cdr.markForCheck()))
    .subscribe({

      next: respuesta => {

        this.servicioSeleccionado =
          respuesta.detalle;

        this.estadosServicio =
          respuesta.estados ?? [];

        this.idEstadoSeleccionado =
          Number(
            respuesta.detalle
              ?.estado
              ?.idEstado ?? 0
          );

        this.observacion = '';

        this.cargandoDetalle = false;
      },

      error: error => {

        console.error(error);

        this.cargandoDetalle = false;

        this.error =
          error?.error?.mensaje ??
          'No fue posible cargar el servicio.';
      }
    });
  }

  cerrarDetalle(): void {

    this.servicioSeleccionado =
      null;

    this.estadosServicio = [];

    this.idEstadoSeleccionado = 0;

    this.observacion = '';
  }

  // ==========================================================
  // ACTUALIZAR ESTADO
  // ==========================================================

  actualizarEstado(): void {

    if (
      !this.servicioSeleccionado
    ) {
      return;
    }

    if (
      this.idEstadoSeleccionado <= 0
    ) {

      this.error =
        'Seleccione el nuevo estado.';

      return;
    }

    const estadoActual =
      Number(
        this.servicioSeleccionado
          ?.estado
          ?.idEstado ?? 0
      );

    if (
      estadoActual ===
      Number(
        this.idEstadoSeleccionado
      )
    ) {

      this.error =
        'Seleccione un estado diferente al estado actual.';

      return;
    }

    this.guardando = true;

    this.error = '';

    this.mensaje = '';

    this.api
      .cambiarEstadoMotorista(
        Number(
          this.servicioSeleccionado
            .idServicio
        ),

        Number(
          this.idEstadoSeleccionado
        ),

        this.observacion
      )
      .pipe(finalize(() => this.cdr.markForCheck()))
    .subscribe({

        next: respuesta => {

          this.guardando = false;

          this.mensaje =
            respuesta?.mensaje ??
            'Estado actualizado correctamente.';

          if (respuesta?.fechaFinalizacion) this.detenerUbicacion();
          this.cerrarDetalle();

          this.recargarDatos();
        },

        error: error => {

          console.error(error);

          this.guardando = false;

          this.error =
            error?.error?.mensaje ??
            'No fue posible actualizar el estado.';
        }
      });
  }

  // ==========================================================
  // RECARGAR DATOS
  // ==========================================================

  recargarDatos(): void {

    forkJoin({
      dashboard:
        this.api.getMotoristaDashboard(),

      servicios:
        this.api.getMotoristaServicios(),

      historial:
        this.api.getMotoristaHistorial()
    })
    .pipe(finalize(() => this.cdr.markForCheck()))
    .subscribe({

      next: respuesta => {

        this.dashboard =
          respuesta.dashboard;

        this.servicios =
          respuesta.servicios ?? [];

        this.historial =
          respuesta.historial ?? [];
      },

      error: error => {

        console.error(error);
      }
    });
  }

  // ==========================================================
  // TEXTO DEL TIPO DE SERVICIO
  // ==========================================================

  tipoServicioTexto(
    tipo: string
  ): string {

    if (
      String(tipo)
        .toUpperCase() ===
      'ENVIO'
    ) {

      return 'Envío';
    }

    return 'Delivery';
  }

  // ==========================================================
  // CERRAR SESIÓN
  // ==========================================================

  cerrarSesion(): void {
    this.detenerUbicacion();

    this.auth.logout();

    this.router.navigate([
      '/login'
    ]);
  }

  compartiendoServicio: number | null = null;
  mensajeUbicacion = '';
  private intervaloUbicacion: ReturnType<typeof setInterval> | null = null;
  private solicitudUbicacion = false;
  private versionUbicacion = 0;

  compartirUbicacion(): void {
    if (!this.servicioSeleccionado?.activo) return;
    this.detenerUbicacion();
    if (!window.isSecureContext) { this.mensajeUbicacion = 'Para usar GPS abre la aplicación mediante HTTPS o localhost.'; return; }
    if (!navigator.geolocation) { this.mensajeUbicacion = 'Este navegador no admite ubicación.'; return; }
    const id = Number(this.servicioSeleccionado.idServicio);
    this.compartiendoServicio = id;
    const version = this.versionUbicacion;
    this.mensajeUbicacion = 'Acepta el permiso de ubicación del navegador. Buscando posición…';
    const enviar = () => {
      if (this.solicitudUbicacion || version !== this.versionUbicacion) return;
      this.solicitudUbicacion = true;
      navigator.geolocation.getCurrentPosition(pos => {
        if (version !== this.versionUbicacion) return;
        this.api.guardarUbicacion(id, pos.coords.latitude, pos.coords.longitude).pipe(finalize(() => {
          if (version === this.versionUbicacion) this.solicitudUbicacion = false;
          this.cdr.markForCheck();
        })).subscribe({
          next: () => { if (version === this.versionUbicacion) this.mensajeUbicacion = 'Ubicación actualizada a las ' + new Date().toLocaleTimeString() + '. Mantén esta página abierta.'; },
          error: error => {
            if (version !== this.versionUbicacion) return;
            if ([401,403,409].includes(error.status)) this.detenerUbicacion();
            this.mensajeUbicacion = error?.error?.mensaje || 'No se pudo enviar la ubicación. Reintentaremos automáticamente.';
          }
        });
      }, error => {
        if (version !== this.versionUbicacion) return;
        this.solicitudUbicacion = false;
        if (error.code === 1) {
          this.detenerUbicacion();
          this.mensajeUbicacion = 'Permiso de ubicación denegado. Actívalo en el navegador y vuelve a pulsar Compartir ubicación.';
        } else this.mensajeUbicacion = 'No se pudo obtener GPS. Comprueba la ubicación del dispositivo; reintentaremos automáticamente.';
        this.cdr.markForCheck();
      }, { enableHighAccuracy: true, maximumAge: 5000, timeout: 12000 });
    };
    enviar();
    this.intervaloUbicacion = setInterval(enviar, 15000);
  }
  detenerUbicacion(): void {
    this.versionUbicacion++;
    if (this.intervaloUbicacion !== null) clearInterval(this.intervaloUbicacion);
    this.intervaloUbicacion = null;
    this.solicitudUbicacion = false;
    this.compartiendoServicio = null;
    this.mensajeUbicacion = 'Ubicación detenida.';
  }
  ngOnDestroy(): void { this.detenerUbicacion(); }
}
