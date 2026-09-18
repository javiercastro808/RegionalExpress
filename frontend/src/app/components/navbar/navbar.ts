import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { map } from 'rxjs';
import { Auth } from '../../services/auth';
import { rutaPanel } from '../../services/sesion-rutas';
@Component({ selector: 'app-navbar', standalone: true, imports: [CommonModule, RouterLink, RouterLinkActive], templateUrl: './navbar.html', styleUrl: './navbar.scss' })
export class Navbar {
  readonly sesion$;
  constructor(private auth: Auth, private router: Router) {
    this.sesion$ = auth.usuario$.pipe(map(usuario => ({ usuario: auth.estaAutenticado() ? usuario : null, panel: rutaPanel(usuario?.rol) })));
  }
  salir(): void { this.auth.logout(); this.router.navigateByUrl('/login'); }
}
