import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

import {
  Auth,
  UsuarioSesion
} from '../../services/auth';

@Component({
  selector: 'app-panel',
  standalone: true,

  imports: [
    CommonModule,
    RouterLink
  ],

  templateUrl: './panel.html',
  styleUrl: './panel.scss'
})
export class Panel {

  usuario: UsuarioSesion | null;

  constructor(
    public auth: Auth,
    private router: Router
  ) {

    this.usuario =
      this.auth.getUsuario();

    if (!this.usuario) {

      this.router.navigate([
        '/login'
      ]);
    }
  }


  get titulo(): string {

    switch (
      this.usuario?.rol
    ) {

      case 'ADMIN_GENERAL':
        return 'Panel administrativo';

      case 'ADMIN_RESTAURANTE':
        return 'Panel del restaurante';

      case 'MOTORISTA':
        return 'Panel del motorista';

      default:
        return 'Regional Express';
    }
  }


  get descripcion(): string {

    switch (
      this.usuario?.rol
    ) {

      case 'ADMIN_GENERAL':

        return 'Administración general de Regional Express.';


      case 'ADMIN_RESTAURANTE':

        return 'Gestiona productos y pedidos del restaurante.';


      case 'MOTORISTA':

        return 'Consulta y administra tus servicios asignados.';


      default:

        return '';
    }
  }


  cerrarSesion(): void {

    this.auth.logout();

    this.router.navigate([
      '/login'
    ]);
  }
}