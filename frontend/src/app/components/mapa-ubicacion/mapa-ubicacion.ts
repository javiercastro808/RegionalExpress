import {
  Input,
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Output,
  ViewChild,
  OnDestroy,
  signal,
} from "@angular/core";
import type * as Leaflet from "leaflet";

export interface PuntoEntrega {
  latitud: number;
  longitud: number;
}
export function coordenadasValidas(lat: number, lng: number): boolean {
  return (
    Number.isFinite(lat) &&
    Number.isFinite(lng) &&
    lat >= -90 &&
    lat <= 90 &&
    lng >= -180 &&
    lng <= 180
  );
}

@Component({
  selector: "app-mapa-ubicacion",
  standalone: true,
  template: `<section aria-label="Ubicación de entrega">
    <p>
      <strong>{{titulo}}.</strong> Usa tu ubicación o
      marca el punto exacto en el mapa. Conserva también la dirección escrita.
    </p>
    <button
      type="button"
      class="btn btn-outline-primary mb-2"
      (click)="localizar()"
      [disabled]="buscando()"
    >
      {{ buscando() ? "Buscando ubicación…" : "Usar mi ubicación" }}
    </button>
    <button type="button" class="btn btn-outline-primary mb-2" (click)="ampliado.set(!ampliado())" [attr.aria-expanded]="ampliado()">{{ampliado() ? "Reducir mapa" : "Ampliar mapa"}}</button>
    <div #canvas [class.ampliado]="ampliado()" class="mapa" aria-label="Selecciona el punto de entrega"></div>
    <p role="status" class="small mt-2">{{ mensaje() }}</p>
  </section>`,
  styles: [
    `
      .mapa {
        height: 300px;
        width: 100%;
        border-radius: 12px;
        z-index: 0;
      }
      .mapa.ampliado { height:70vh; height:70dvh; min-height:300px; }
      p {
        font-size: 14px;
      }
    `,
  ],
})
export class MapaUbicacion implements AfterViewInit, OnDestroy {
  ampliado = signal(false);
  @Input() titulo='Ubicación obligatoria para la entrega';
  @Input() solicitarGps=true;
  @Output() puntoChange = new EventEmitter<PuntoEntrega>();
  @ViewChild("canvas", { static: true }) canvas!: ElementRef<HTMLDivElement>;
  buscando = signal(false);
  mensaje = signal(
    "El mapa inicial es una referencia; selecciona tu punto de entrega.",
  );
  private libreria?: typeof Leaflet;
  private mapa?: Leaflet.Map;
  private marcador?: Leaflet.CircleMarker;
  private resize?: ResizeObserver;
  private destruido = false;
  private punto?: PuntoEntrega;
  async ngAfterViewInit() {
    try {
      const L = await import("leaflet");
      if (this.destruido) return;
      this.libreria = L;
      this.mapa = L.map(this.canvas.nativeElement).setView(
        [14.609, -90.535],
        11,
      );
      L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution:
          '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
      }).addTo(this.mapa);
      this.mapa.on("click", (event: Leaflet.LeafletMouseEvent) =>
        this.elegir(event.latlng.lat, event.latlng.lng),
      );
      if (typeof ResizeObserver !== "undefined") {
        this.resize = new ResizeObserver(() => this.mapa?.invalidateSize());
        this.resize.observe(this.canvas.nativeElement);
      }
      if (this.punto) this.dibujar();
      if(this.solicitarGps)this.localizar();
    } catch {
      if (!this.destruido) {
        this.mensaje.set(
          "No se pudo cargar el mapa. Intenta obtener tu ubicación con el botón o recarga la página.",
        );
      }
    }
  }
  elegir(lat: number, lng: number) {
    if (this.destruido || !coordenadasValidas(lat, lng)) return;
    this.punto = { latitud: lat, longitud: lng };
    this.dibujar();
    this.puntoChange.emit(this.punto);
    this.mensaje.set(
      "Punto seleccionado: " + lat.toFixed(6) + ", " + lng.toFixed(6),
    );
  }
  private dibujar() {
    if (!this.mapa || !this.libreria || !this.punto) return;
    const p: Leaflet.LatLngExpression = [
      this.punto.latitud,
      this.punto.longitud,
    ];
    if (this.marcador) this.marcador.setLatLng(p);
    else
      this.marcador = this.libreria
        .circleMarker(p, { radius: 9, fillOpacity: 1, color: "#2563eb" })
        .addTo(this.mapa);
    this.mapa.setView(p, 16);
  }
  localizar() {
    if (this.buscando()) return;
    if (!navigator.geolocation) {
      this.mensaje.set(
        "El navegador no admite ubicación. Selecciona un punto en el mapa.",
      );
      return;
    }
    this.buscando.set(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        if (this.destruido) return;
        this.buscando.set(false);
        this.elegir(pos.coords.latitude, pos.coords.longitude);
      },
      (error) => {
        if (this.destruido) return;
        this.buscando.set(false);
        this.mensaje.set(
          error.code === 1
            ? "Permiso denegado. Puedes seleccionar el punto manualmente."
            : "No se pudo obtener la ubicación. Selecciona el punto manualmente.",
        );
      },
      { enableHighAccuracy: true, timeout: 15000, maximumAge: 30000 },
    );
  }
  ngOnDestroy() {
    this.destruido = true;
    this.resize?.disconnect();
    this.mapa?.remove();
  }
}
