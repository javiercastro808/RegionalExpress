import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from './auth';
export function rutaPanel(rol: string | null | undefined): string {
  switch (String(rol ?? '').trim().toUpperCase()) {
    case 'ADMIN_GENERAL': case 'ADMINISTRADOR_GENERAL': return '/panel/admin';
    case 'ADMIN_RESTAURANTE': case 'ADMINISTRADOR_RESTAURANTE': return '/panel/restaurante';
    case 'MOTORISTA': return '/panel/motorista';
    case 'CLIENTE': return '/panel/cliente';
    default: return '/login';
  }
}
export const soloInvitados: CanActivateFn = () => {
  const auth = inject(Auth), router = inject(Router);
  const destino = rutaPanel(auth.getUsuario()?.rol);
  return auth.estaAutenticado() && destino !== '/login' ? router.parseUrl(destino) : true;
};
export const panelAutorizado: CanActivateFn = route => {
  const auth = inject(Auth), router = inject(Router);
  if (!auth.estaAutenticado()) { auth.logout(); return router.parseUrl('/login'); }
  const destino = rutaPanel(auth.getUsuario()?.rol);
  return destino === '/' + route.routeConfig?.path ? true : router.parseUrl(destino);
};
