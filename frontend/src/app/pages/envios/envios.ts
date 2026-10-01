import { MapaUbicacion, PuntoEntrega } from '../../components/mapa-ubicacion/mapa-ubicacion';
import { ChangeDetectorRef } from '@angular/core';
import { finalize } from 'rxjs';
import { Auth } from "../../services/auth";
import {
  Component
} from '@angular/core';

import {
  CommonModule
} from '@angular/common';

import {
  FormsModule
} from '@angular/forms';

import {
  RouterLink
} from '@angular/router';

import {
  Api
} from '../../services/api';


@Component({
  selector: 'app-envios',

  standalone: true,

  imports: [
    CommonModule,
    FormsModule,
    RouterLink, MapaUbicacion
  ],

  templateUrl:
    './envios.html',

  styleUrl:
    './envios.scss'
})
export class Envios {
  puntoOrigen:PuntoEntrega|null=null;
  puntoDestino:PuntoEntrega|null=null;

  // ==========================================
  // REMITENTE
  // ==========================================

  nombreRemitente = '';

  telefonoRemitente = '';

  correoRemitente = '';


  // ==========================================
  // DESTINATARIO
  // ==========================================

  nombreDestinatario = '';

  telefonoDestinatario = '';

  correoDestinatario = '';


  // ==========================================
  // ORIGEN
  // ==========================================

  direccionOrigen = '';

  referenciaOrigen = '';


  // ==========================================
  // DESTINO
  // ==========================================

  direccionDestino = '';

  referenciaDestino = '';


  // ==========================================
  // PAQUETE
  // ==========================================

  peso: number | null = null;

  distanciaKm: number | null = null;

  tipoPaquete = 'CAJA';

  descripcionPaquete = '';


  // ==========================================
  // TARIFA
  // ==========================================

  tarifaCalculada: any = null;

  calculando = false;


  // ==========================================
  // ENVÍO
  // ==========================================

  procesando = false;

  envioConfirmado = false;

  codigoRastreo = '';
  correoConfirmacion = '';

  mensaje = '';

  error = '';


  constructor(
    private cdr: ChangeDetectorRef,
    private auth: Auth,
    private api: Api
  ) {}


  // ==========================================
  // CALCULAR TARIFA
  // ==========================================

  calcularTarifa(): void {

    this.error = '';

    this.mensaje = '';

    this.tarifaCalculada = null;


    if (
      this.peso === null ||
      Number(this.peso) <= 0
    )
    {
      this.error =
        'Ingresa un peso válido.';

      return;
    }


    if (
      this.distanciaKm === null ||
      Number(this.distanciaKm) < 0
    )
    {
      this.error =
        'Ingresa una distancia válida.';

      return;
    }


    this.calculando = true;


    this.api
      .calcularEnvio(
        Number(this.peso),
        Number(this.distanciaKm)
      )
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({

        next: respuesta => {

          this.tarifaCalculada =
            respuesta;

          this.calculando =
            false;

        },


        error: error => {

          console.error(
            'Error calculando tarifa:',
            error
          );


          this.calculando =
            false;


          this.error =
            error?.error?.mensaje
            ??
            'No se pudo calcular la tarifa.';

        }

      });

  }


  // ==========================================
  // VALIDAR FORMULARIO
  // ==========================================

  validarFormulario(): boolean {

    this.error = '';


    if (
      this.nombreRemitente
        .trim() === ''
    )
    {
      this.error =
        'Ingresa el nombre del remitente.';

      return false;
    }


    if (
      this.telefonoRemitente
        .trim() === ''
    )
    {
      this.error =
        'Ingresa el teléfono del remitente.';

      return false;
    }


    if (
      this.nombreDestinatario
        .trim() === ''
    )
    {
      this.error =
        'Ingresa el nombre del destinatario.';

      return false;
    }


    if (
      this.telefonoDestinatario
        .trim() === ''
    )
    {
      this.error =
        'Ingresa el teléfono del destinatario.';

      return false;
    }


    if (
      this.direccionOrigen
        .trim() === ''
    )
    {
      this.error =
        'Ingresa la dirección de origen.';

      return false;
    }


    if (
      this.direccionDestino
        .trim() === ''
    )
    {
      this.error =
        'Ingresa la dirección de destino.';

      return false;
    }


    if (
      this.peso === null ||
      Number(this.peso) <= 0
    )
    {
      this.error =
        'Ingresa el peso del paquete.';

      return false;
    }


    if (
      this.distanciaKm === null ||
      Number(this.distanciaKm) < 0
    )
    {
      this.error =
        'Ingresa la distancia aproximada.';

      return false;
    }


    if (
      this.tipoPaquete
        .trim() === ''
    )
    {
      this.error =
        'Selecciona el tipo de paquete.';

      return false;
    }


    return true;

  }


  // ==========================================
  // CONFIRMAR ENVÍO
  // ==========================================

  confirmarEnvio(): void {
    if(!this.puntoOrigen||!this.puntoDestino){this.error='Marca en el mapa el punto de recogida y el punto de entrega.';return;}
    if (this.procesando) return;
    if (!this.auth.estaAutenticado() || !this.auth.tieneRol("CLIENTE")) { this.error = "Inicia sesión con una cuenta de cliente para confirmar tu envío."; return; }

    this.error = '';

    this.mensaje = '';


    if (
      !this.validarFormulario()
    )
    {
      return;
    }


    if (
      !this.tarifaCalculada
    )
    {
      this.error =
        'Primero debes calcular la tarifa.';

      return;
    }


    const envio = {
      latitudOrigen:this.puntoOrigen.latitud,longitudOrigen:this.puntoOrigen.longitud,
      latitudDestino:this.puntoDestino.latitud,longitudDestino:this.puntoDestino.longitud,

      idCliente: this.auth.getUsuario()!.idUsuario,


      nombreRemitente:
        this.nombreRemitente
          .trim(),


      telefonoRemitente:
        this.telefonoRemitente
          .trim(),


      correoRemitente:
        this.correoRemitente
          .trim() === ''
            ? null
            : this.correoRemitente
                .trim(),


      nombreDestinatario:
        this.nombreDestinatario
          .trim(),


      telefonoDestinatario:
        this.telefonoDestinatario
          .trim(),


      correoDestinatario:
        this.correoDestinatario
          .trim() === ''
            ? null
            : this.correoDestinatario
                .trim(),


      direccionOrigen:
        this.direccionOrigen
          .trim(),


      referenciaOrigen:
        this.referenciaOrigen
          .trim() === ''
            ? null
            : this.referenciaOrigen
                .trim(),


      direccionDestino:
        this.direccionDestino
          .trim(),


      referenciaDestino:
        this.referenciaDestino
          .trim() === ''
            ? null
            : this.referenciaDestino
                .trim(),


      peso:
        Number(this.peso),


      distanciaKm:
        Number(
          this.distanciaKm
        ),


      tipoPaquete:
        this.tipoPaquete,


      descripcionPaquete:
        this.descripcionPaquete
          .trim() === ''
            ? null
            : this.descripcionPaquete
                .trim()

    };


    this.procesando = true;


    this.api
      .crearEnvio(envio)
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({

        next: respuesta => {


          this.procesando =
            false;


          this.envioConfirmado =
            true;


          this.codigoRastreo =
            respuesta.codigoRastreo;
          this.correoConfirmacion = respuesta.correoConfirmacion || 'No configurado';


          this.mensaje =
            'Envío registrado correctamente.';

        },


        error: error => {

          console.error(
            'Error registrando envío:',
            error
          );


          this.procesando =
            false;


          this.error =
            error?.error?.mensaje
            ??
            'No se pudo registrar el envío.';

        }

      });

  }


  // ==========================================
  // MODIFICAR DATOS
  // ==========================================

  datosModificados(): void {

    this.tarifaCalculada = null;

    this.error = '';

    this.mensaje = '';

  }


  // ==========================================
  // NUEVO ENVÍO
  // ==========================================

  nuevoEnvio(): void {
    this.puntoOrigen=null;this.puntoDestino=null;

    this.nombreRemitente = '';

    this.telefonoRemitente = '';

    this.correoRemitente = '';


    this.nombreDestinatario = '';

    this.telefonoDestinatario = '';

    this.correoDestinatario = '';


    this.direccionOrigen = '';

    this.referenciaOrigen = '';

    this.direccionDestino = '';

    this.referenciaDestino = '';


    this.peso = null;

    this.distanciaKm = null;

    this.tipoPaquete = 'CAJA';

    this.descripcionPaquete = '';


    this.tarifaCalculada = null;

    this.envioConfirmado = false;

    this.codigoRastreo = '';

    this.mensaje = '';

    this.error = '';

  }

}