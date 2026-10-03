import { VentanaMapa } from '../../components/ventana-mapa/ventana-mapa';
import { capaMapa } from '../../services/capa-mapa';
import {signal} from '@angular/core';
import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, ViewChild } from '@angular/core';
import * as L from 'leaflet';
import { puntosRastreo, necesitaEncuadre } from './puntos-rastreo';
@Component({selector:'app-mapa-rastreo',standalone:true,imports:[VentanaMapa],template:'<div #mapa class="mapa" aria-label="Origen, motorista y destino"></div><app-ventana-mapa [elemento]="mapa" /> <button type="button" (click)="verTodos()">Ver todos los puntos</button><p role="alert">{{errorMapa()}}</p><p>Azul: origen · Verde: motorista · Rojo: destino. Las posiciones GPS son aproximadas. Los puntos no representan una ruta por carretera.</p>',styles:['.mapa{height:360px;width:100%;border-radius:12px;z-index:0}.mapa.ampliado{height:75vh;height:75dvh;min-height:360px}button{padding:10px 14px;margin-top:10px;border:1px solid #16614e;border-radius:8px;background:#fff;color:#16614e}p{font-size:13px;margin-top:10px}']})
export class MapaRastreo implements AfterViewInit,OnChanges,OnDestroy{
 errorMapa=signal('');
 @Input() ubicacion:any;
 @ViewChild('mapa',{static:true}) elemento!:ElementRef<HTMLDivElement>;
 private mapa?:L.Map;private puntos?:L.LayerGroup;private resize?:ResizeObserver;private tiposEncuadrados="";
 ngAfterViewInit(){this.mapa=L.map(this.elemento.nativeElement).setView([14.61,-90.53],10);this.puntos=L.layerGroup().addTo(this.mapa);capaMapa(L,mensaje=>this.errorMapa.set(mensaje)).addTo(this.mapa);if(typeof ResizeObserver!=='undefined'){this.resize=new ResizeObserver(()=>{this.mapa?.invalidateSize();this.verTodos();});this.resize.observe(this.elemento.nativeElement);}this.actualizar();}
 ngOnChanges(){this.actualizar();}
 verTodos(){
  const puntos=puntosRastreo(this.ubicacion);
  if(!this.mapa||!puntos.length)return;
  this.mapa.fitBounds(L.latLngBounds(puntos.map(p=>[p.latitud,p.longitud] as L.LatLngTuple)),{padding:[90,50],maxZoom:16});
  this.tiposEncuadrados=puntos.map(p=>p.tipo).join(',');
 }
 private actualizar(){
  if(!this.mapa||!this.puntos)return;
  this.puntos.clearLayers();
  const puntos=puntosRastreo(this.ubicacion);
  for(const p of puntos)L.circleMarker([p.latitud,p.longitud],{radius:9,color:p.color,fillOpacity:1})
   .bindTooltip(p.etiqueta,{permanent:true,direction:p.tipo==='motorista'?'top':'bottom'}).addTo(this.puntos);
  if(necesitaEncuadre(this.tiposEncuadrados,puntos))this.verTodos();
 }
 ngOnDestroy(){this.resize?.disconnect();this.mapa?.remove();}
}
