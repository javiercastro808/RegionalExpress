import { rutaPanel } from '../../services/sesion-rutas';
import { ChangeDetectorRef } from '@angular/core';
import { finalize } from 'rxjs';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { Auth } from '../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {

  correo = '';
  password = '';

  mostrarPassword = false;

  cargando = false;
  error = '';

  constructor(
    private cdr: ChangeDetectorRef,
    private auth: Auth,
    private router: Router
  ) {}

  // ==========================================================
  // INICIAR SESIÓN
  // ==========================================================

  iniciarSesion(): void {

    if (this.cargando) return;
    this.error = ''; 

    if (
      !this.correo.trim() ||
      !this.password
    ) {

      this.error =
        'Ingresa tu correo y contraseña.';

      return;
    }

    this.cargando = true;

    this.auth
      .login(
        this.correo.trim(),
        this.password
      )
      .pipe(finalize(() => this.cdr.markForCheck()))
      .subscribe({

        next: respuesta => {

          this.cargando = false;

          const rol =
            String(
              respuesta?.usuario?.rol ?? ''
            )
            .trim()
            .toUpperCase();

          this.redireccionarPorRol(
            rol
          );
        },

        error: error => {

          console.error(
            'Error al iniciar sesión:',
            error
          );

          this.cargando = false;

          this.error =
            error?.error?.mensaje ??
            'No fue posible iniciar sesión.';
        }
      });
  }

  // ==========================================================
  // REDIRECCIÓN POR ROL
  // ==========================================================

  redireccionarPorRol(
    rol: string
  ): void {

    const destino = rutaPanel(rol);
    if (destino === '/login') { this.auth.logout(); this.error = 'El usuario no posee un rol válido.'; return; }
    this.router.navigateByUrl(destino);
  }

  // ==========================================================
  // MOSTRAR / OCULTAR CONTRASEÑA
  // ==========================================================

  alternarPassword(): void {

    this.mostrarPassword =
      !this.mostrarPassword;
  }
}