import { ChangeDetectorRef, Component, OnInit, OnDestroy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { forkJoin, finalize, Subscription, timer, exhaustMap, filter, catchError, of } from "rxjs";

import { Api } from "../../services/api";
import { Auth, UsuarioSesion } from "../../services/auth";

@Component({
  selector: "app-motorista",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./motorista.html",
  styleUrl: "./motorista.scss",
})
export class Motorista implements OnInit, OnDestroy {
  private novedades?:Subscription;
  avisoServicios='';
  usuario: UsuarioSesion | null = null;

  seccion: "inicio" | "servicios" | "delivery" | "envios" | "historial" =
    "inicio";

  dashboard: any = null;

  servicios: any[] = [];

  historial: any[] = [];

  servicioSeleccionado: any = null;

  estadosServicio: any[] = [];

  idEstadoSeleccionado = 0;

  observacion = "";

  cargando = true;

  cargandoDetalle = false;

  guardando = false;

  mensaje = "";

  error = "";

  constructor(
    private cdr: ChangeDetectorRef,
    private api: Api,
    private auth: Auth,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.usuario = this.auth.getUsuario();

    if (!this.usuario) {
      this.router.navigate(["/login"]);

      return;
    }

    const rol = String(this.usuario.rol ?? "").toUpperCase();

    if (rol !== "MOTORISTA") {
      this.router.navigate(["/login"]);

      return;
    }

    this.cargarTodo();
    this.novedades=timer(20000,20000).pipe(filter(()=>!document.hidden&&!this.guardando&&!this.cargando),exhaustMap(()=>this.api.getMotoristaServicios().pipe(catchError(()=>of(null))))).subscribe(datos=>{
      if(datos){const anteriores=new Set(this.servicios.map(s=>Number(s.idServicio)));const nuevos=datos.filter(s=>!anteriores.has(Number(s.idServicio))&&s.activo&&!s.fechaFinalizacion);
      this.servicios=datos;
      if(nuevos.length)this.avisoServicios='Tienes '+nuevos.length+' nuevo(s) servicio(s) asignado(s). Revisa Servicios.';
      if(this.compartiendoServicio!==null&&!datos.some(s=>Number(s.idServicio)===this.compartiendoServicio&&s.activo&&!s.fechaFinalizacion))this.detenerUbicacion();
      }this.cdr.markForCheck();
    });
  }

  // ==========================================================
  // CARGA GENERAL
  // ==========================================================

  cargarTodo(): void {
    this.cargando = true;

    this.error = "";

    forkJoin({
      dashboard: this.api.getMotoristaDashboard(),

      servicios: this.api.getMotoristaServicios(),

      historial: this.api.getMotoristaHistorial(),
    })
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({
        next: (respuesta) => {
          this.dashboard = respuesta.dashboard;

          this.servicios = respuesta.servicios ?? [];

          this.historial = respuesta.historial ?? [];

          this.cargando = false;
        },

        error: (error) => {
          console.error(error);

          this.cargando = false;

          if (error.status === 401 || error.status === 403) {
            this.auth.logout();

            this.router.navigate(["/login"]);

            return;
          }

          this.error =
            error?.error?.mensaje ??
            "No fue posible cargar el panel del motorista.";
        },
      });
  }

  // ==========================================================
  // NAVEGACIÓN INTERNA
  // ==========================================================

  cambiarSeccion(
    seccion: "inicio" | "servicios" | "delivery" | "envios" | "historial",
  ): void {
    this.seccion = seccion;
    this.soloActivos = false;

    this.error = "";

    this.mensaje = "";

    this.cerrarDetalle();
  }

  // ==========================================================
  // FILTROS
  // ==========================================================

  get serviciosActivos(): any[] {
    return this.servicios.filter(
      (servicio) => servicio.activo && !servicio.fechaFinalizacion,
    );
  }

  get serviciosDelivery(): any[] {
    return this.servicios.filter(
      (servicio) => String(servicio.tipoServicio).toUpperCase() === "DELIVERY",
    );
  }

  get serviciosEnvio(): any[] {
    return this.servicios.filter(
      (servicio) => String(servicio.tipoServicio).toUpperCase() === "ENVIO",
    );
  }

  soloActivos = false;
  verResumen(seccion: "servicios" | "delivery" | "envios", activos = false): void {
    this.cambiarSeccion(seccion); this.soloActivos = activos;
  }
  get tituloServicios(): string {
    return this.soloActivos ? "Servicios activos" : this.seccion === "delivery" ? "Entregas de comida" : this.seccion === "envios" ? "Envíos de paquetes" : "Todos mis servicios";
  }
  get serviciosVista(): any[] {
    if (this.soloActivos) return this.serviciosActivos;
    if (this.seccion === "delivery") {
      return this.serviciosDelivery;
    }

    if (this.seccion === "envios") {
      return this.serviciosEnvio;
    }

    return this.servicios;
  }

  // ==========================================================
  // DETALLE DEL SERVICIO
  // ==========================================================

  verServicio(servicio: any): void {
    const idServicio = Number(servicio.idServicio);

    if (!idServicio) {
      return;
    }

    this.cargandoDetalle = true;

    this.error = "";

    this.mensaje = "";

    forkJoin({
      detalle: this.api.getMotoristaServicio(idServicio),

      estados: this.api.getMotoristaEstados(idServicio),
    })
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({
        next: (respuesta) => {
          this.servicioSeleccionado = respuesta.detalle;

          this.estadosServicio = respuesta.estados ?? [];

          this.idEstadoSeleccionado = Number(this.siguientePaso?.idEstado ?? 0);

          this.observacion = "";
          if (
            this.servicioSeleccionado?.activo &&
            !this.servicioSeleccionado?.fechaFinalizacion &&
            this.compartiendoServicio !==
              Number(this.servicioSeleccionado.idServicio)
          )
            this.compartirUbicacion();

          this.cargandoDetalle = false;
        },

        error: (error) => {
          console.error(error);

          this.cargandoDetalle = false;

          this.error =
            error?.error?.mensaje ?? "No fue posible cargar el servicio.";
        },
      });
  }

  cerrarDetalle(): void {
    this.servicioSeleccionado = null;

    this.estadosServicio = [];

    this.idEstadoSeleccionado = 0;

    this.observacion = "";
  }

  // ==========================================================
  // ACTUALIZAR ESTADO
  // ==========================================================

  get pasosEntrega(): any[] {
    return this.estadosServicio.filter(e=>!String(e.nombreEstado).toUpperCase().includes('CANCEL')).slice().sort((a,b)=>Number(a.ordenEstado)-Number(b.ordenEstado)||Number(a.idEstado)-Number(b.idEstado));
  }
  get siguientePaso(): any {
    const orden=Number(this.servicioSeleccionado?.estado?.ordenEstado??0);
    return this.pasosEntrega.find(e=>Number(e.ordenEstado)>orden);
  }
  confirmarPaso(): void {
    if(!this.siguientePaso)return;
    this.idEstadoSeleccionado=Number(this.siguientePaso.idEstado);
    this.actualizarEstado();
  }
  actualizarEstado(): void {
    if (this.guardando) return;
    if (this.requiereUbicacion() && !this.ubicacionLista()) {
      this.error =
        "Debes activar la ubicación y esperar la confirmación del servidor antes de avanzar la entrega.";
      if (
        this.compartiendoServicio !==
        Number(this.servicioSeleccionado?.idServicio)
      )
        this.compartirUbicacion();
      return;
    }

    if (!this.servicioSeleccionado) {
      return;
    }

    if (this.idEstadoSeleccionado <= 0) {
      this.error = "Seleccione el nuevo estado.";

      return;
    }

    const estadoActual = Number(
      this.servicioSeleccionado?.estado?.idEstado ?? 0,
    );

    if (estadoActual === Number(this.idEstadoSeleccionado)) {
      this.error = "Seleccione un estado diferente al estado actual.";

      return;
    }

    this.guardando = true;

    this.error = "";

    this.mensaje = "";

    this.api
      .cambiarEstadoMotorista(
        Number(this.servicioSeleccionado.idServicio),

        Number(this.idEstadoSeleccionado),

        this.observacion,
      )
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({
        next: (respuesta) => {
          this.guardando = false;

          this.mensaje =
            respuesta?.mensaje ?? "Estado actualizado correctamente.";

          if (respuesta?.fechaFinalizacion) this.detenerUbicacion();
          if (respuesta?.fechaFinalizacion) this.cerrarDetalle();
          else {
            this.servicioSeleccionado = { ...this.servicioSeleccionado, estado: respuesta.estado };
            this.idEstadoSeleccionado = Number(this.siguientePaso?.idEstado ?? 0);
            this.observacion = "";
          }
          this.recargarDatos();
        },

        error: (error) => {
          console.error(error);

          this.guardando = false;

          this.error =
            error?.error?.mensaje ?? "No fue posible actualizar el estado.";
        },
      });
  }

  // ==========================================================
  // RECARGAR DATOS
  // ==========================================================

  recargarDatos(): void {
    forkJoin({
      dashboard: this.api.getMotoristaDashboard(),

      servicios: this.api.getMotoristaServicios(),

      historial: this.api.getMotoristaHistorial(),
    })
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({
        next: (respuesta) => {
          this.dashboard = respuesta.dashboard;

          this.servicios = respuesta.servicios ?? [];

          this.historial = respuesta.historial ?? [];
        },

        error: (error) => {
          console.error(error);
        },
      });
  }

  // ==========================================================
  // TEXTO DEL TIPO DE SERVICIO
  // ==========================================================

  tipoServicioTexto(tipo: string): string {
    if (String(tipo).toUpperCase() === "ENVIO") {
      return "Envío";
    }

    return "Entrega de comida";
  }

  // ==========================================================
  // CERRAR SESIÓN
  // ==========================================================

  cerrarSesion(): void {
    this.detenerUbicacion();

    this.auth.logout();

    this.router.navigate(["/login"]);
  }

  compartiendoServicio: number | null = null;
  mensajeUbicacion = "";
  ultimaUbicacionConfirmada = 0;
  private ciclo?: Subscription;
  private envio?: Subscription;
  private generacion = 0;
  private consultando = false;
  private esperaGps?: ReturnType<typeof setTimeout>;
  requiereUbicacion(): boolean {
    const estado = this.estadosServicio.find(
      (e) => Number(e.idEstado) === Number(this.idEstadoSeleccionado),
    );
    const modalidad =
      this.servicioSeleccionado?.detalle?.modalidad ??
      this.servicioSeleccionado?.pedido?.modalidadEntrega ??
      this.servicioSeleccionado?.pedido?.modalidad;
    return (
      !!this.servicioSeleccionado &&
      modalidad !== "RECOGER" &&
      !String(estado?.nombreEstado ?? "")
        .toUpperCase()
        .includes("CANCEL")
    );
  }
  ubicacionLista(): boolean {
    return (
      this.compartiendoServicio ===
        Number(this.servicioSeleccionado?.idServicio) &&
      this.ultimaUbicacionConfirmada > 0 &&
      Date.now() - this.ultimaUbicacionConfirmada < 120000
    );
  }
  compartirUbicacion(): void {
    if (
      !this.servicioSeleccionado?.activo ||
      this.servicioSeleccionado?.fechaFinalizacion
    )
      return;
    this.detenerUbicacion();
    if (!window.isSecureContext) {
      this.mensajeUbicacion = "Abre la página con el enlace HTTPS compartido o localhost para permitir la ubicación.";
      return;
    }
    if (!navigator.geolocation) {
      this.mensajeUbicacion =
        "Este navegador no permite obtener ubicación. Usa un dispositivo con GPS.";
      return;
    }
    const id = Number(this.servicioSeleccionado.idServicio),
      generacion = this.generacion;
    this.compartiendoServicio = id;
    this.mensajeUbicacion =
      "Buscando tu ubicación. Si aparece la solicitud del navegador, permite el acceso.";
    this.ciclo = timer(0, 15000).subscribe(() => {
      this.cdr.markForCheck();
      if (this.consultando) return;
      this.consultando = true;
      let terminado = false;
      const completar = () => {
        if (terminado || generacion !== this.generacion) return false;
        terminado = true;
        clearTimeout(this.esperaGps);
        return true;
      };
      this.esperaGps = setTimeout(() => {
        if (!completar()) return;
        this.detenerUbicacion();
        this.mensajeUbicacion = "El dispositivo no respondió con una ubicación. Revisa la ubicación del sistema o abre el enlace HTTPS en tu teléfono con GPS activo y pulsa Reintentar ubicación.";
        this.cdr.markForCheck();
      }, 20000);
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          if (!completar()) return;
          if (
            !Number.isFinite(pos.coords.latitude) ||
            !Number.isFinite(pos.coords.longitude) ||
            !Number.isFinite(pos.coords.accuracy) ||
            pos.coords.accuracy < 0 ||
            pos.coords.accuracy > 500 ||
            Math.abs(pos.coords.latitude) > 90 || Math.abs(pos.coords.longitude) > 180 ||
            !Number.isFinite(pos.timestamp) ||
            Date.now() - pos.timestamp > 30000
          ) {
            this.consultando = false;
            this.ultimaUbicacionConfirmada = 0;
            this.mensajeUbicacion =
              `La ubicación recibida tiene una precisión de ${Math.round(pos.coords.accuracy)} metros. Se requiere una posición reciente con precisión de 500 metros o mejor. Usa el teléfono con GPS activo; volveremos a intentarlo.`;
            this.cdr.markForCheck();
            return;
          }
          this.mensajeUbicacion = "Ubicación obtenida. Esperando confirmación del servidor...";
          this.cdr.markForCheck();
          this.envio = this.api
            .guardarUbicacion(id, pos.coords.latitude, pos.coords.longitude)
            .pipe(
              finalize(() => {
                if (generacion === this.generacion) {
                  this.consultando = false;
                  this.cdr.markForCheck();
                }
              }),
            )
            .subscribe({
              next: () => {
                if (generacion !== this.generacion) return;
                this.ultimaUbicacionConfirmada = Date.now();
                this.mensajeUbicacion =
                  `Ubicación confirmada${pos.coords.accuracy > 150 ? " (aproximada, margen de " + Math.round(pos.coords.accuracy) + " metros)" : ""}. Mantén esta página abierta durante la entrega.`;
              },
              error: (error) => {
                if (generacion !== this.generacion) return;
                this.ultimaUbicacionConfirmada = 0;
                if ([401, 403, 409].includes(error.status))
                  this.detenerUbicacion();
                this.mensajeUbicacion =
                  error?.error?.mensaje ||
                  "No se está enviando tu ubicación. Revisa la conexión; volveremos a intentarlo.";
                this.cdr.markForCheck();
              },
            });
        },
        (error) => {
          if (!completar()) return;
          this.consultando = false;
          this.ultimaUbicacionConfirmada = 0;
          if (error.code === 1) this.detenerUbicacion();
          this.mensajeUbicacion =
            error.code === 1
              ? "Ubicación denegada. Actívala en los permisos del navegador y pulsa Compartir ubicación para continuar."
              : "El dispositivo no pudo obtener coordenadas. Revisa la ubicación del sistema o usa el enlace HTTPS en tu teléfono con GPS activo. Reintentaremos automáticamente.";
          this.cdr.markForCheck();
        },
        { enableHighAccuracy: true, timeout: 12000, maximumAge: 5000 },
      );
    });
  }
  detenerUbicacion(): void {
    this.generacion++;
    clearTimeout(this.esperaGps);
    this.ciclo?.unsubscribe();
    this.envio?.unsubscribe();
    this.consultando = false;
    this.compartiendoServicio = null;
    this.ultimaUbicacionConfirmada = 0;
    this.mensajeUbicacion =
      "Ubicación detenida. Actívala antes de continuar la entrega.";
  }
  ngOnDestroy(): void {
    this.novedades?.unsubscribe();
    this.detenerUbicacion();
  }
}
