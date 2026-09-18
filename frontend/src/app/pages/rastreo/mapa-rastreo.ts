import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, ViewChild } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-mapa-rastreo',
  standalone: true,
  template: '<div class="map-actions"><button type="button" (click)="centrar()">Centrar en el motorista</button><span>{{ ubicacion?.reciente ? "Ubicación reciente" : "Última posición recibida" }}</span></div><div #mapa class="mapa" aria-label="Última ubicación del motorista"></div>',
  styles: ['.mapa { height: 360px; width: 100%; border-radius: 12px; z-index: 0; } .map-actions { display:flex; flex-wrap:wrap; align-items:center; gap:12px; margin:12px 0; font-size:13px; } button { border:1px solid #bdcdc5; border-radius:8px; padding:8px 12px; background:#fff; color:#184332; }']
})
export class MapaRastreo implements AfterViewInit, OnChanges, OnDestroy {
  @Input() ubicacion: any;
  @ViewChild('mapa', { static: true }) elemento!: ElementRef<HTMLDivElement>;
  private mapa?: L.Map;
  private marcador?: L.CircleMarker;
  ngAfterViewInit(): void {
    this.mapa = L.map(this.elemento.nativeElement);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>', maxZoom: 19
    }).addTo(this.mapa);
    this.actualizar();
  }
  ngOnChanges(): void { this.actualizar(); }
  centrar(): void {
    if (this.marcador) this.mapa?.setView(this.marcador.getLatLng(), this.mapa.getZoom() || 15);
  }
  private actualizar(): void {
    if (!this.mapa || !this.ubicacion?.disponible) return;
    const latitud = Number(this.ubicacion.latitud), longitud = Number(this.ubicacion.longitud);
    if (!Number.isFinite(latitud) || !Number.isFinite(longitud) || Math.abs(latitud) > 90 || Math.abs(longitud) > 180) return;
    const punto: L.LatLngExpression = [latitud, longitud];
    if (!this.marcador) {
      this.marcador = L.circleMarker(punto, { radius: 10, color: '#166534', fillColor: '#22c55e', fillOpacity: 1 }).addTo(this.mapa);
      this.mapa.setView(punto, 15);
    } else this.marcador.setLatLng(punto);
    this.marcador.setStyle({ color: this.ubicacion.reciente ? '#166534' : '#a15c00', fillColor: this.ubicacion.reciente ? '#22c55e' : '#f6aa1c' });
  }
  ngOnDestroy(): void { this.mapa?.remove(); }
}
