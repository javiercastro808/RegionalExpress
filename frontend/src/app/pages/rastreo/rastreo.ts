import { Subscription, timer, forkJoin, of, catchError, exhaustMap } from 'rxjs';
import { MapaRastreo } from './mapa-rastreo';
import {
  ChangeDetectorRef, Component, OnDestroy, OnInit
} from '@angular/core';

import {
  CommonModule
} from '@angular/common';

import {
  FormsModule
} from '@angular/forms';

import {
  ActivatedRoute, RouterLink
} from '@angular/router';

import {
  Api
} from '../../services/api';


@Component({
  selector: 'app-rastreo',

  standalone: true,

  imports: [
    CommonModule,
    FormsModule,
    RouterLink, MapaRastreo
  ],

  templateUrl:
    './rastreo.html',

  styleUrl:
    './rastreo.scss'
})
export class Rastreo implements OnInit, OnDestroy {

  codigo = '';

  resultado: any = null;

  buscando = false;

  error = '';


  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private api: Api
  ) {}


  // ==========================================
  // BUSCAR
  // ==========================================

  ubicacion: any = null;
  avisoActualizacion = '';
  ultimaActualizacion: Date | null = null;
  private consulta?: Subscription;
  private parametros?: Subscription;
  ngOnInit(): void {
    this.parametros = this.route.queryParamMap.subscribe(params => {
      const codigo = params.get('codigo');
      if (codigo) { this.codigo = codigo; this.buscar(); }
    });
  }

  buscar(): void {
    this.consulta?.unsubscribe();
    this.resultado = null;
    this.ubicacion = null;
    this.error = '';
    this.avisoActualizacion = '';
    this.ultimaActualizacion = null;
    this.buscando = false;
    const codigo = this.codigo.trim().toUpperCase();
    if (!codigo) { this.error = 'Ingresa un código de rastreo.'; return; }
    this.buscando = true;
    this.consulta = timer(0, 15000).pipe(exhaustMap(() => forkJoin({
      servicio: this.api.rastrear(codigo),
      ubicacion: this.api.getUbicacionRastreo(codigo).pipe(catchError(() => of(null)))
    }).pipe(catchError(() => of(null))))).subscribe(datos => {
      this.buscando = false;
      if (datos) {
        this.resultado = datos.servicio;
        this.ubicacion = datos.ubicacion;
        this.error = '';
        this.avisoActualizacion = datos.ubicacion === null ? 'No se pudo consultar la ubicación.' : '';
        this.ultimaActualizacion = new Date();
        if (datos.servicio?.activo === false || datos.servicio?.fechaFinalizacion) this.consulta?.unsubscribe();
      } else if (this.resultado) this.avisoActualizacion = 'No se pudo actualizar. Mostrando la última información recibida; reintentaremos automáticamente.';
      else { this.error = 'No se encontró el servicio o no hay conexión con el servidor.'; this.consulta?.unsubscribe(); }
      this.cdr.markForCheck();
    });
  }
  ngOnDestroy(): void { this.consulta?.unsubscribe(); this.parametros?.unsubscribe(); }

  // ==========================================
  // LIMPIAR
  // ==========================================

  limpiar(): void {
    this.consulta?.unsubscribe();
    this.ubicacion = null;
    this.buscando = false;
    this.avisoActualizacion = '';

    this.codigo = '';

    this.resultado = null;

    this.error = '';

  }


  // ==========================================
  // TIPO DELIVERY
  // ==========================================

  esDelivery(): boolean {

    return (
      this.resultado
        ?.tipoServicio
        ?.toUpperCase()
      ===
      'DELIVERY'
    );

  }


  // ==========================================
  // TIPO ENVIO
  // ==========================================

  esEnvio(): boolean {

    return (
      this.resultado
        ?.tipoServicio
        ?.toUpperCase()
      ===
      'ENVIO'
    );

  }


  // ==========================================
  // ESTADO ALCANZADO
  // ==========================================

  estadoAlcanzado(
    orden: number
  ): boolean {

    if (!this.resultado)
    {
      return false;
    }


    return (
      Number(
        this.resultado
          .ordenEstado
      ) >= orden
    );

  }


  // ==========================================
  // MODALIDAD DELIVERY
  // ==========================================

  modalidadTexto(): string {

    if (
      this.resultado
        ?.modalidad ===
      'RECOGER'
    )
    {
      return 'Pasar a recoger';
    }


    return 'Entrega a domicilio';

  }


  // ==========================================
  // TOTAL
  // ==========================================

  total(): number {

    return Number(
      this.resultado
        ?.total ?? 0
    );

  }


  // ==========================================
  // TEXTO DEL TIPO
  // ==========================================

  tipoTexto(): string {

    if (
      this.esDelivery()
    )
    {
      return 'Pedido Delivery';
    }


    if (
      this.esEnvio()
    )
    {
      return 'Envío de paquete';
    }


    return 'Servicio';

  }


  // ==========================================
  // ICONO
  // ==========================================

  tipoIcono(): string {

    if (
      this.esDelivery()
    )
    {
      return '🍔';
    }


    if (
      this.esEnvio()
    )
    {
      return '📦';
    }


    return '📍';

  }

}