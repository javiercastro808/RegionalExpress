import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { finalize } from 'rxjs/operators';

import { Api } from '../../services/api';

@Component({
  selector: 'app-delivery',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './delivery.html',
  styleUrl: './delivery.scss'
})
export class Delivery implements OnInit {

  restaurantes: any[] = [];

  cargando = true;

  error = '';

  constructor(
    private api: Api,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.cargarRestaurantes();
  }

  cargarRestaurantes(): void {

    this.cargando = true;
    this.error = '';

    this.api
      .getRestaurantes()
      .pipe(
        finalize(() => {

          this.cargando = false;

          this.cdr.detectChanges();

        })
      )
      .subscribe({

        next: (datos: any[]) => {

          console.log(
            'Restaurantes recibidos:',
            datos
          );

          this.restaurantes = datos;

          this.cdr.detectChanges();

        },

        error: (error) => {

          console.error(
            'Error cargando restaurantes:',
            error
          );

          this.error =
            'No se pudieron cargar los restaurantes.';

          this.cdr.detectChanges();

        }

      });

  }

}